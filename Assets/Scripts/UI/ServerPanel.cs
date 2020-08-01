using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ServerPanel : MonoBehaviour
{
	public Button stopButton = null;
	public ClientInfoUI serverInfoUI = null;
    public ClientListUI clientListUI = null;
	public LogViewUI logViewUI = null;

	private void Start()
	{
		stopButton.onClick.AddListener(() =>
		{
			OnlineManager.StopServer();
		});

		OnlineManager.ConnectionCallback += (Client client) =>
		{
			clientListUI.AddClient(client.SocketInfo.RemoteIpAddress.ToString(), client.SocketInfo.RemotePort.ToString());
		};

		OnlineManager.DisconnectionCallBack += (Client client) =>
		{
			clientListUI.RemoveClient(client.SocketInfo.RemoteIpAddress.ToString(), client.SocketInfo.RemotePort.ToString());
		};

		OnlineManager.LogCallback += (string log) =>
		{
			logViewUI.AddLog(log);
		};

		OnlineManager.StartServer();

		var tmp = OnlineManager.GetSocketInfo();
		serverInfoUI.SetIpAndPort(tmp.LocalIpAddress.ToString(), tmp.LocalPort.ToString());
	}
}
