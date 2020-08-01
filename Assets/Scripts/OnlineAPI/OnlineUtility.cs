using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public static class OnlineUtility
{
	public static IPAddress GetLocalIPV4Address()
	{
		var ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

		foreach (var address in ipHostEntry.AddressList)
		{
			if (address.AddressFamily == AddressFamily.InterNetwork)
			{
				return address;
			}
		}

		return null;
	}
	public static IPAddress GetLocalIPV6Address()
	{
		var ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());

		foreach (var address in ipHostEntry.AddressList)
		{
			if (address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				return address;
			}
		}

		return null;
	}

	public static bool IsConnectedSocket(Socket socket)
	{
		if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
		{
			return false;
		}

		return true;
	}
}
