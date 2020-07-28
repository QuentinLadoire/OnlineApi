using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientPanel : MonoBehaviour
{
    [SerializeField] Button disConnectedButton = null;

	private void Start()
	{
		disConnectedButton.onClick.AddListener(() =>
		{
			OnlineManager.DisconnectClient();
		});
	}
}
