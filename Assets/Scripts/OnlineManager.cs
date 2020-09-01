using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum OnlineType
{
	None = -1,
	Server,
	Client
}

public enum OnlinePlayerId
{
	None = -1,
	Server,
	Player1,
	Player2,
	Player3,
	Player4,
	Player5,
	Player6,
	Player7,
	Player8,
	Player9,
	Player10,
	Count
}

public enum MsgProtocol
{
	None = -1,
	PlayerConnection,
	InstantiateObject,
	DestroyObject
}

public class InstantiateObjectInfo
{
	public GameObject GameObject { get; private set; }
	public Vector3 Position { get; private set; }
	public Quaternion Rotation { get; private set; }
	public int Id { get; private set; }
	public OnlinePlayerId PlayerOwner { get; private set; }

	public InstantiateObjectInfo()
	{
		GameObject = null;
		Position = Vector3.zero;
		Rotation = Quaternion.identity;
		Id = -1;
		PlayerOwner = OnlinePlayerId.None;
	}
	public InstantiateObjectInfo(GameObject gameObject, Vector3 position, Quaternion rotation, int id, OnlinePlayerId playerOwner)
	{
		GameObject = gameObject;
		Position = position;
		Rotation = rotation;
		Id = id;
		PlayerOwner = playerOwner;
	}
	public InstantiateObjectInfo(byte[] bytes)
	{
		var ms = new MemoryStream(bytes);
		var br = new BinaryReader(ms);

		GameObject = OnlinePrefabs.GetPrefabBy(br.ReadString());
		Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		Rotation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
		Id = br.ReadInt32();
		PlayerOwner = (OnlinePlayerId)br.ReadInt32();

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

		bw.Write((int)PlayerOwner);

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
	static OnlinePlayerId playerIdGenerator = OnlinePlayerId.None;

	OnlineType onlineType = OnlineType.None;

	OnlinePlayerId playerId = OnlinePlayerId.None;

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
		instance.playerId = GeneratePlayerId();

		instance.server = new TcpServer();

		instance.server.connectionCallBack += instance.ServerConnectionCallback;
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
			instance.server.connectionCallBack -= instance.ServerConnectionCallback;
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

		return instance.client.ConnectTo("192.168.1.14", 8000);
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
	public static bool IsOwner(OnlinePlayerId playerId)
	{
		return instance.playerId == playerId;
	}
	public static OnlinePlayerId GetPlayerId()
	{
		return instance.playerId;
	}
	public static void SendMsg(byte[] bytes)
	{
		if (instance.onlineType == OnlineType.Server)
		{
			instance.server.SendMsg(bytes);
		}
		else if (instance.onlineType == OnlineType.Client)
		{
			instance.client.SendMsg(bytes);
		}
	}
	public static void SendMsg(byte[] bytes, int[] clientIds)
	{
		if (instance.onlineType == OnlineType.Server)
		{
			instance.server.SendMsg(bytes, clientIds);
		}
	}

	public static void Instantiate(InstantiateObjectInfo info)
	{
		var onlineIdentifiant = Instantiate(info.GameObject, info.Position, info.Rotation).GetComponent<OnlineIdentifiant>();
		onlineIdentifiant.Set(info.Id, info.PlayerOwner);

		OnlineObjectManager.AddObject(onlineIdentifiant);
	}
	public static void Destroy(DestroyObjectInfo info)
	{
		OnlineObjectManager.DestroyObject(info.Id);
	}

	static OnlinePlayerId GeneratePlayerId()
	{
		playerIdGenerator++;

		return playerIdGenerator;
	}

	void ServerConnectionCallback(Client client)
	{
		byte[] bytes = BitConverter.GetBytes((int)MsgProtocol.PlayerConnection).Concat(BitConverter.GetBytes((int)GeneratePlayerId())).ToArray();

		SendMsg(bytes, new int[]{ client.Id });
	}

	void ServerReceiveMsgCallback(byte[] bytes)
	{
		var msgType = (MsgProtocol)BitConverter.ToInt32(bytes, 0);
		switch (msgType)
		{
			case MsgProtocol.InstantiateObject:
				var instantiateInfo = new InstantiateObjectInfo(bytes.Skip(sizeof(int)).ToArray());
				this.OnlineInstantiate(instantiateInfo.GameObject, instantiateInfo.Position, instantiateInfo.Rotation, instantiateInfo.PlayerOwner);
				break;

			case MsgProtocol.DestroyObject:
				var destroyInfo = new DestroyObjectInfo(bytes.Skip(sizeof(int)).ToArray());
				this.OnlineDestroy(OnlineObjectManager.GetObjectBy(destroyInfo.Id).gameObject);
				break;
		}
	}
	void ClientReceiveMsgCallback(byte[] bytes)
	{
		var msgType = (MsgProtocol)BitConverter.ToInt32(bytes, 0);
		switch (msgType)
		{
			case MsgProtocol.PlayerConnection:
				instance.playerId = (OnlinePlayerId)BitConverter.ToInt32(bytes, sizeof(int));
				break;

			case MsgProtocol.InstantiateObject:
				Instantiate(new InstantiateObjectInfo(bytes.Skip(sizeof(int)).ToArray()));
				break;

			case MsgProtocol.DestroyObject:
				Destroy(new DestroyObjectInfo(bytes.Skip(sizeof(int)).ToArray()));
				break;

			case MsgProtocol.None:
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
