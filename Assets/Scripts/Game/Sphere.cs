using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
	OnlineIdentifiant onlineId = null;

	private void Awake()
	{
		onlineId = GetComponent<OnlineIdentifiant>();
	}
	private void Update()
	{
		if (OnlineManager.IsOwner(onlineId.PlayerOwner))
		{
			if (Input.GetKeyDown(KeyCode.Delete))
			{
				this.OnlineDestroy(gameObject);
			}
		}
	}
}
