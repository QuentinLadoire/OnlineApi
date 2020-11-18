using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tmp : MonoBehaviour
{
	[SerializeField] ClientPanel clientPanel = null;
	[SerializeField] ServerPanel serverPanel = null;

	private void Awake()
	{
		Screen.SetResolution(800, 450, FullScreenMode.Windowed);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			if (!OnlineManager.IsHost() && clientPanel != null) clientPanel.gameObject.SetActive(!clientPanel.gameObject.activeSelf);
			if (OnlineManager.IsHost() && serverPanel != null) serverPanel.gameObject.SetActive(!serverPanel.gameObject.activeSelf);
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.OnlineInstantiate(OnlinePrefabs.GetPrefabBy("Sphere"), Vector3.zero, Quaternion.identity, OnlineManager.GetPlayerId());
		}
	}
}
