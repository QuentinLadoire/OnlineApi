using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
	float time = 3.0f;

	OnlineIdentifiant onlineId = null;

	private void Awake()
	{
		onlineId = GetComponent<OnlineIdentifiant>();

		GetComponent<Renderer>().material.color = Random.ColorHSV();
	}

	private void Update()
	{
		if (OnlineManager.IsHost())
		{
			if (time < 0.0f)
			{
				this.OnlineDestroy(onlineId.Id);
			}
			time -= Time.deltaTime;
		}
	}
}
