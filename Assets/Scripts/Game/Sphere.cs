using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Delete))
		{
			this.OnlineDestroy(gameObject);
		}
	}
}
