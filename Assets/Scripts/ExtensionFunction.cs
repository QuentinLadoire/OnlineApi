﻿using System;
using System.Linq;
using UnityEngine;

public static class ExtensionFunction
{
	public static void OnlineInstantiate(this UnityEngine.Object obj, GameObject gameObject, Vector3 position, Quaternion rotation, OnlinePlayerId playerOwner)
	{
		InstantiateObjectInfo info = null;

		if (OnlineManager.IsHost()) info = new InstantiateObjectInfo(gameObject, position, rotation, OnlineObjectManager.GenerateId(), playerOwner);
		else info = new InstantiateObjectInfo(gameObject, position, rotation, -1, playerOwner);

		var bytes = BitConverter.GetBytes((int)MsgProtocol.InstantiateObject).Concat(info.Serialize()).ToArray();
		OnlineManager.SendMsg(bytes);

		if (OnlineManager.IsHost()) OnlineManager.Instantiate(info);
	}

	public static void OnlineDestroy(this UnityEngine.Object obj, GameObject gameObject)
	{
		var onlineId = gameObject.GetComponent<OnlineIdentifiant>();
		if (onlineId != null)
		{
			var info = new DestroyObjectInfo(onlineId.ObjectId);
			var bytes = BitConverter.GetBytes((int)MsgProtocol.DestroyObject).Concat(info.Serialize()).ToArray();

			OnlineManager.SendMsg(bytes);

			if (OnlineManager.IsHost()) OnlineManager.Destroy(info);
		}
	}
}
