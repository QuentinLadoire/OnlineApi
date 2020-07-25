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

	#region Instance Variable

	OnlineType onlineType = OnlineType.None;

	TcpServer server = null;
	TcpClient client = null;

	#endregion

	#region Static Function

	public static DequeueCallback DequeueCallback { get => instance.server.dequeueCallBack; set => instance.server.dequeueCallBack = value; }
	public static ConnectionCallback ConnectionCallback { get => instance.server.connectionCallBack; set => instance.server.connectionCallBack = value; }

	public static void StartServer()
	{
		instance.onlineType = OnlineType.Server;

		instance.server = new TcpServer();
		instance.server.Start();

		instance.server.dequeueCallBack += (string msg) => { Debug.Log(msg); };
	}
	public static void ConnectClient()
	{
		instance.onlineType = OnlineType.Client;

		instance.client = new TcpClient();
		instance.client.ConnectTo(new IPAddress(3232235790), 8000);
	}

	#endregion

	#region Instance Function

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
	}

	#endregion
}
