using System;
using System.Collections;
using System.Collections.Generic;

public class Message
{
	public int ClientId { get; private set; }

	public byte[] Bytes { get; private set; }

	public Message(int clientId, byte[] bytes, int lenght)
	{
		ClientId = clientId;

		Bytes = new byte[lenght];
		Array.Copy(bytes, Bytes, lenght);
	}
}
