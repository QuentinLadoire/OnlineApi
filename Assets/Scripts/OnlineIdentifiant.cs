using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineIdentifiant : MonoBehaviour
{
    public int ObjectId { get; private set; }
	public OnlinePlayerId PlayerOwner { get; private set; }

    public void Set(int objectId, OnlinePlayerId playerOwner)
	{
		ObjectId = objectId;
		PlayerOwner = playerOwner;
	}
}
