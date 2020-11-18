using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(OnlineIdentifiant))]
public class OnlineComponent : MonoBehaviour
{
	public MsgProtocol MsgProtocol { get; protected set; }

    protected OnlineIdentifiant onlineID = null;

	protected virtual void Start()
	{
		onlineID = GetComponent<OnlineIdentifiant>();
	}

	protected virtual byte[] Serialize()
	{
		return null;
	}
	protected virtual void Deserialize(byte[] bytes)
	{
		
	}

	public void SendMSG()
	{
		OnlineManager.SendMsg(Serialize());
	}
	public void ReadMSG(byte[] msg)
	{
		if (OnlineManager.IsHost()) OnlineManager.SendMsg(msg);

		if (!OnlineManager.IsOwner(onlineID.PlayerOwner)) Deserialize(msg);
	}
}
