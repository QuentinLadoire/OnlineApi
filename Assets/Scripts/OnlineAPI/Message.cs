using System;
using System.Collections;
using System.Collections.Generic;

public class Message
{
	public int Id { get; private set; }

	public byte[] Bytes { get; private set; }

	public Message(int id, byte[] bytes, int lenght)
	{
		Id = id;

		Bytes = new byte[lenght];
		Array.Copy(bytes, Bytes, lenght);
	}
}
