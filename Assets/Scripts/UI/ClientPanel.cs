using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientPanel : MonoBehaviour
{
	[SerializeField] MenuPanelUI menuPanel = null;
	[SerializeField] ClientInfoUI clientInfoUI = null;
	[SerializeField] ClientInfoUI connectedServerInfoUI = null;
    [SerializeField] Button disConnectedButton = null;
	[SerializeField] LogViewUI logViewUI = null;

	private void Start()
	{
		var tmp = OnlineManager.GetSocketInfo();
		clientInfoUI.SetIpAndPort(tmp.LocalIpAddress.ToString(), tmp.LocalPort.ToString());
		connectedServerInfoUI.SetIpAndPort(tmp.RemoteIpAddress.ToString(), tmp.RemotePort.ToString());

		disConnectedButton.onClick.AddListener(() =>
		{
			OnlineManager.DisconnectClient();

			gameObject.SetActive(false);
			menuPanel.gameObject.SetActive(true);
		});

		OnlineManager.LogCallback += (string log) =>
		{
			logViewUI.AddLog(log);
		};
	}
}
