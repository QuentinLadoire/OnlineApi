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

			int axisX = 0;
			if (Input.GetKey(KeyCode.D))
			{
				axisX += 1;
			}
			if (Input.GetKey(KeyCode.Q))
			{
				axisX += -1;
			}

			int axisY = 0;
			if (Input.GetKey(KeyCode.Z))
			{
				axisY += 1;
			}
			if (Input.GetKey(KeyCode.S))
			{
				axisY += -1;
			}

			transform.position += new Vector3(axisX, axisY, 0.0f).normalized * 20 * Time.deltaTime;
		}
	}
}
