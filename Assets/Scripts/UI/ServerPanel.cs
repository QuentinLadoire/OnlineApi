using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ServerPanel : MonoBehaviour
{
	public MenuPanelUI menuPanel = null;
	public Button quitButton = null;
	public Button stopButton = null;
	public ClientInfoUI serverInfoUI = null;
    public ClientListUI clientListUI = null;
	public LogViewUI logViewUI = null;

	private void Start()
	{
		quitButton.onClick.AddListener(() =>
		{
			OnlineManager.DestroyServer();

			gameObject.SetActive(false);
			menuPanel.gameObject.SetActive(true);
		});

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
	}

	private void OnEnable()
	{
		OnlineManager.StartServer();

		var tmp = OnlineManager.GetSocketInfo();
		serverInfoUI.SetIpAndPort(tmp.LocalIpAddress.ToString(), tmp.LocalPort.ToString());
	}
}
