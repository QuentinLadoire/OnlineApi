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
				var str = ipEndPoint.Address.ToString();
				if (!clientListUI.Contains(str))
				{
					clientListUI.AddClient(str);
				}
			}
		};
	}
}
