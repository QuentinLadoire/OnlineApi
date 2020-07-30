using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
	public byte[] Bytes { get; private set; }

	public Message(byte[] bytes, int lenght)
	{
		Bytes = new byte[lenght];
		Array.Copy(bytes, Bytes, lenght);
	}
}
