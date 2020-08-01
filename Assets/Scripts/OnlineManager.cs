using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public enum OnlineType
{
	None = -1,
	Server,
	Client
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

	public static bool ConnectClient()
	{
		instance.onlineType = OnlineType.Client;

		instance.client = new TcpClient();

		return instance.client.ConnectTo(IPAddress.Parse("192.168.1.14"), 8000);
	}
	public static void DisconnectClient()
	{
		if (instance.onlineType == OnlineType.Client)
		{
			instance.client.Close();
		}
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

			if (Input.GetKeyDown(KeyCode.M))
			{
				client.SendMsg();
			}
		}
	}
}
