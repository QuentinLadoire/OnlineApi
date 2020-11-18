using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineIdentifiant : MonoBehaviour
{
    public int ObjectId { get; private set; }
	public OnlinePlayerId PlayerOwner { get; private set; }

	List<OnlineComponent> onlineComponents = null;

    public void Set(int objectId, OnlinePlayerId playerOwner)
	{
		ObjectId = objectId;
		PlayerOwner = playerOwner;
	}

	public void ReadMSG(MsgProtocol msgProtocol, byte[] msg)
	{
		if (onlineComponents.Count > 0)
		{
			onlineComponents.Find(item => item.MsgProtocol == msgProtocol).ReadMSG(msg);
		}
	}

	private void Start()
	{
		onlineComponents = new List<OnlineComponent>(GetComponents<OnlineComponent>());
	}
}
