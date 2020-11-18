using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineTransformComponent : OnlineComponent
{
	[SerializeField] bool replicateRotation = false;
	[SerializeField] bool replicateScale = false;

	Vector3 onlinePosition = Vector3.zero;
	Quaternion onlineRotation = Quaternion.identity;
	Vector3 onlineScale = Vector3.zero;

	Vector3 lastFramePosition = Vector3.zero;

	protected override byte[] Serialize()
	{
		var ms = new MemoryStream();
		var bw = new BinaryWriter(ms);

		bw.Write((int)MsgProtocol);
		bw.Write(onlineID.ObjectId);

		bw.Write(transform.position.x);
		bw.Write(transform.position.y);
		bw.Write(transform.position.z);

		if (replicateRotation)
		{
			bw.Write(transform.rotation.x);
			bw.Write(transform.rotation.y);
			bw.Write(transform.rotation.z);
			bw.Write(transform.rotation.w);
		}

		if (replicateScale)
		{
			bw.Write(transform.localScale.x);
			bw.Write(transform.localScale.y);
			bw.Write(transform.localScale.z);
		}

		var bytes = ms.ToArray();

		bw.Close();
		ms.Close();

		return bytes;
	}
	protected override void Deserialize(byte[] bytes)
	{
		var ms = new MemoryStream(bytes);
		var br = new BinaryReader(ms);

		br.ReadInt32();
		br.ReadInt32();

		onlinePosition.x = br.ReadSingle();
		onlinePosition.y = br.ReadSingle();
		onlinePosition.z = br.ReadSingle();

		if (replicateRotation)
		{
			onlineRotation.x = br.ReadSingle();
			onlineRotation.y = br.ReadSingle();
			onlineRotation.z = br.ReadSingle();
			onlineRotation.w = br.ReadSingle();
		}

		if (replicateScale)
		{
			onlineScale.x = br.ReadSingle();
			onlineScale.y = br.ReadSingle();
			onlineScale.z = br.ReadSingle();
		}

		br.Close();
		ms.Close();
	}

	protected override void Start()
	{
		base.Start();

		MsgProtocol = MsgProtocol.TransformReplication;
	}

	private void Update()
	{
		if (OnlineManager.IsOwner(onlineID.PlayerOwner)) return;

		transform.position = onlinePosition;
		if (replicateRotation) transform.rotation = onlineRotation;
		if (replicateScale) transform.localScale = onlineScale;
	}

	private void LateUpdate()
	{
		if (OnlineManager.IsOwner(onlineID.PlayerOwner))
		{
			if (lastFramePosition != transform.position) SendMSG();
		}

		lastFramePosition = transform.position;
	}
}
