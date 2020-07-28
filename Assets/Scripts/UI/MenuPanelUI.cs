using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanelUI : MonoBehaviour
{
    [SerializeField] GameObject serverPanel = null;
    [SerializeField] GameObject clientPanel = null;

    [SerializeField] Button serverButton = null;
    [SerializeField] Button clientButton = null;

	private void Awake()
	{
		if (serverButton != null) serverButton.onClick.AddListener(() =>
		{
			OnlineManager.StartServer();

			gameObject.SetActive(false);
			serverPanel.SetActive(true);
		});

		if (clientButton != null) clientButton.onClick.AddListener(() =>
		{
			if (OnlineManager.ConnectClient())
			{

				gameObject.SetActive(false);
				clientPanel.SetActive(true);
			}
		});
	}
}
