using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ServerPanel : MonoBehaviour
{
    public ClientListUI clientListUI = null;

	private void Start()
	{
		OnlineManager.ConnectionCallback += (IPEndPoint[] ipEndPoints) =>
		{
			foreach (var ipEndPoint in ipEndPoints)
			{
				var ip = ipEndPoint.Address.ToString();
				var port = ipEndPoint.Port.ToString();

				if (!clientListUI.Contains(ip, port))
				{
					clientListUI.AddClient(ip, port);
				}
			}
		};
	}
}
