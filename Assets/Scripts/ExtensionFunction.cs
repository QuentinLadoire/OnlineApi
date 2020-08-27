using System;
using System.Linq;
using UnityEngine;

public static class ExtensionFunction
{
	public static void OnlineInstantiate(this UnityEngine.Object obj, GameObject gameObject, Vector3 position, Quaternion rotation)
	{
		var info = new InstantiateObjectInfo(gameObject, position, rotation, OnlineObjectManager.GenerateId());
		var bytes = BitConverter.GetBytes((int)MsgType.InstantiateObject).Concat(info.Serialize()).ToArray();

		OnlineManager.SendMsg(bytes);

		OnlineManager.Instantiate(info);
	}

	public static void OnlineDestroy(this UnityEngine.Object obj, int id)
	{
		var info = new DestroyObjectInfo(id);
		var bytes = BitConverter.GetBytes((int)MsgType.DestroyObject).Concat(info.Serialize()).ToArray();

		OnlineManager.SendMsg(bytes);

		OnlineManager.Destroy(info);
	}
}
