using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerPanel : MonoBehaviour
{
	public ClientInfoUI serverInfoUI = null;
    public ClientListUI clientListUI = null;
	public LogViewUI logViewUI = null;

	private void Start()
	{
		OnlineManager.ConnectionCallback += (Client client) =>
		{
			clientListUI.AddClient(client.RemoteIpAddress.ToString(), client.RemotePort.ToString());
		};

		OnlineManager.DisconnectionCallBack += (Client client) =>
		{
			clientListUI.RemoveClient(client.RemoteIpAddress.ToString(), client.RemotePort.ToString());
		};

		OnlineManager.LogCallback += (string log) =>
		{
			logViewUI.AddLog(log);
		};

		OnlineManager.StartServer();

		var tmp = OnlineManager.GetClientInfo();
		serverInfoUI.SetIpAndPort(tmp.LocalIpAddress.ToString(), tmp.LocalPort.ToString());
	}
}
