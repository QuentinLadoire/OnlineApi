using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlinePrefabs : MonoBehaviour
{
    static OnlinePrefabs instance = null;

	[SerializeField] List<GameObject> prefabs = new List<GameObject>();

	private void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	public static GameObject GetPrefabBy(string name)
	{
		foreach (var obj in instance.prefabs)
		{
			if (obj.name == name)
			{
				return obj;
			}
		}

		return null;
	}
}
