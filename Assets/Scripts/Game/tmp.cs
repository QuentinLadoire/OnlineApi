﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tmp : MonoBehaviour
{
	[SerializeField] ClientPanel clientPanel = null;

	private void Awake()
	{
		Screen.SetResolution(800, 450, FullScreenMode.Windowed);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{
			if (clientPanel != null) clientPanel.gameObject.SetActive(!clientPanel.gameObject.activeSelf);
		}

		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.OnlineInstantiate(OnlinePrefabs.GetPrefabBy("Sphere"), Vector3.zero, Quaternion.identity);
		}
	}
}
