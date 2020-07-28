using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerPanel : MonoBehaviour
{
    public ClientListUI clientListUI = null;
	public LogViewUI logViewUI = null;

	private void Start()
	{
		OnlineManager.ConnectionCallback += (Client client) =>
		{
			clientListUI.AddClient(client.IpAddress.ToString(), client.Port.ToString());
		};

		OnlineManager.LogCallback += (string log) =>
		{
			logViewUI.AddLog(log);
		};
	}
}
