using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public enum OnlineType
{
	None = -1,
	Server,
	Client
}

public enum MsgType
{
	None = -1,
	InstantiateObject,
	DestroyObject
}

public class InstantiateObjectInfo
{
	public GameObject GameObject { get; private set; }
	public Vector3 Position { get; private set; }
	public Quaternion Rotation { get; private set; }
	public int Id { get; private set; }

	public InstantiateObjectInfo()
	{
		GameObject = null;
		Position = Vector3.zero;
		Rotation = Quaternion.identity;
		Id = -1;
	}
	public InstantiateObjectInfo(GameObject gameObject, Vector3 position, Quaternion rotation, int id)
	{
		GameObject = gameObject;
		Position = position;
		Rotation = rotation;
		Id = id;
	}
	public InstantiateObjectInfo(byte[] bytes)
	{
		var ms = new MemoryStream(bytes);
		var br = new BinaryReader(ms);

		GameObject = OnlinePrefabs.GetPrefabBy(br.ReadString());
		Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		Rotation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		Id = br.ReadInt32();

		br.Close();
		ms.Close();
	}

	public byte[] Serialize()
	{
		var ms = new MemoryStream();
		var bw = new BinaryWriter(ms);

		bw.Write(GameObject.name);

		bw.Write(Position.x);
		bw.Write(Position.y);
		bw.Write(Position.z);

		bw.Write(Rotation.x);
		bw.Write(Rotation.y);
		bw.Write(Rotation.z);
		bw.Write(Rotation.w);

		bw.Write(Id);

		var bytes = ms.ToArray();

		bw.Close();
		ms.Close();

		return bytes;
	}
}
public class DestroyObjectInfo
{
	public int Id { get; private set; }

	public DestroyObjectInfo()
	{
		Id = -1;
	}
	public DestroyObjectInfo(int id)
	{
		Id = id;
	}
	public DestroyObjectInfo(byte[] bytes)
	{
		var ms = new MemoryStream(bytes);
		var br = new BinaryReader(ms);

		Id = br.ReadInt32();

		br.Close();
		ms.Close();
	}

	public byte[] Serialize()
	{
		var ms = new MemoryStream();
		var bw = new BinaryWriter(ms);

		bw.Write(Id);

		var bytes = ms.ToArray();

		bw.Close();
		ms.Close();

		return bytes;
	}
}

public class OnlineManager : MonoBehaviour
{
	static OnlineManager instance = null;

	OnlineType onlineType = OnlineType.None;

	TcpServer server = null;
	TcpClient client = null;

	public static LogCallback LogCallback
	{
		get
		{
			if (instance.onlineType == OnlineType.Server) return instance.server.logCallback;
			else if (instance.onlineType == OnlineType.Client) return instance.client.logCallback;

			return null;
		}
		set
		{
			if (instance.onlineType == OnlineType.Server) instance.server.logCallback = value;
			else if (instance.onlineType == OnlineType.Client) instance.client.logCallback = value;
		}
	}
	public static ConnectionCallback ConnectionCallback { get => instance.server.connectionCallBack; set => instance.server.connectionCallBack = value; }
	public static ConnectionCallback DisconnectionCallBack { get => instance.server.disConnectionCallBack; set => instance.server.disConnectionCallBack = value; }

	public static void CreateServer()
	{
		instance.onlineType = OnlineType.Server;

		instance.server = new TcpServer();

		instance.server.receiveMsgCallBack += instance.ServerReceiveMsgCallback;
	}
	public static void StartServer()
	{
		if (instance.onlineType == OnlineType.Server)
		{
			instance.server.Start();
		}
	}
	public static void StopServer()
	{
		if (instance.onlineType == OnlineType.Server)
		{
			instance.server.Close();
		}
	}
	public static void DestroyServer()
	{
		if (instance.onlineType == OnlineType.Server)
		{
			instance.server.receiveMsgCallBack -= instance.ServerReceiveMsgCallback;

			instance.server = null;
			instance.onlineType = OnlineType.None;
		}
	}

	public static bool ConnectClient()
	{
		instance.onlineType = OnlineType.Client;

		instance.client = new TcpClient();

		instance.client.receiveMsgCallBack += instance.ClientReceiveMsgCallback;

		return instance.client.ConnectTo(IPAddress.Parse("192.168.1.14"), 8000);
	}
	public static void DisconnectClient()
	{
		if (instance.onlineType == OnlineType.Client)
		{
			instance.client.Close();

			instance.client.receiveMsgCallBack -= instance.ClientReceiveMsgCallback;
			instance.client = null;
			instance.onlineType = OnlineType.None;
		}
	}
	public static bool ClientIsDisconnected()
	{
		if (instance.onlineType == OnlineType.Client)
		{
			return instance.client.IsConnected;
		}

		return false;
	}

	public static SocketInfo GetSocketInfo()
	{
		if (instance.onlineType == OnlineType.Server)
		{
			return instance.server.GetSocketInfo();
		}
		else if (instance.onlineType == OnlineType.Client)
		{
			return instance.client.GetSocketInfo();
		}

		return null;
	}
	public static bool IsHost()
	{
		return instance.onlineType == OnlineType.Server;
	}
	public static void SendMsg(byte[] bytes)
	{
		if (instance.onlineType == OnlineType.Server)
		{
			instance.server.SendMsg(bytes);
		}
		else if (instance.onlineType == OnlineType.Client)
		{
			instance.server.SendMsg(bytes);
		}
	}

	public static void Instantiate(InstantiateObjectInfo info)
	{
		var onlineIdentifiant = Instantiate(info.GameObject, info.Position, info.Rotation).GetComponent<OnlineIdentifiant>();
		onlineIdentifiant.Set(info.Id);

		OnlineObjectManager.AddObject(onlineIdentifiant);
	}
	public static void Destroy(DestroyObjectInfo info)
	{
		OnlineObjectManager.DestroyObject(info.Id);
	}

	void ServerReceiveMsgCallback(byte[] bytes)
	{

	}
	void ClientReceiveMsgCallback(byte[] bytes)
	{
		var msgType = (MsgType)BitConverter.ToInt32(bytes, 0);
		switch (msgType)
		{
			case MsgType.InstantiateObject:
				Instantiate(new InstantiateObjectInfo(bytes.Skip(sizeof(int)).ToArray()));
				break;

			case MsgType.DestroyObject:
				Destroy(new DestroyObjectInfo(bytes.Skip(sizeof(int)).ToArray()));
				break;

			case MsgType.None:
				break;
		}
	}

	private void Awake()
	{
		if (instance == null) instance = this;
	}
	private void Update()
	{
		if (onlineType == OnlineType.Server)
		{
			server.Update();
		}
		else if (onlineType == OnlineType.Client)
		{
			client.Update();
		}
	}
}
