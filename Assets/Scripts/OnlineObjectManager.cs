using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineObjectManager
{
	static OnlineObjectManager instance = null;
	static OnlineObjectManager Instance
	{
		get
		{
			if (instance == null) instance = new OnlineObjectManager();
			return instance;
		}
	}

    List<OnlineIdentifiant> onlineObjects = new List<OnlineIdentifiant>();

	int idGenerator = 0;

	public static int GenerateId()
	{
		int id = Instance.idGenerator;
		Instance.idGenerator++;

		return id;
	}
	public static void AddObject(OnlineIdentifiant onlineId)
	{
		Instance.onlineObjects.Add(onlineId);
	}
	public static OnlineIdentifiant RemoveObject(int id)
	{
		var toRemove = Instance.onlineObjects.Find(item => item.Id == id);
		Instance.onlineObjects.Remove(toRemove);

		return toRemove;
	}
	public static void DestroyObject(int id)
	{
		var toDestroy = RemoveObject(id);
		GameObject.Destroy(toDestroy.gameObject);
	}
	public static OnlineIdentifiant GetObjectBy(int id)
	{
		foreach (var obj in Instance.onlineObjects)
		{
			if (obj.Id == id) return obj;
		}

		return null;
	}
}
