using System.Net;
using System.Net.Sockets;

public class Client
{
	public Socket Socket { get; private set; }

	public IPAddress RemoteIpAddress { get; private set; }
	public int RemotePort { get; private set; }

	public IPAddress LocalIpAddress { get; private set; }
	public int LocalPort { get; private set; }

	public Client(Socket socket)
	{
		this.Socket = socket;

		var ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
		if (ipEndPoint != null)
		{
			RemoteIpAddress = ipEndPoint.Address;
			RemotePort = ipEndPoint.Port;
		}

		ipEndPoint = socket.LocalEndPoint as IPEndPoint;
		if (ipEndPoint != null)
		{
			LocalIpAddress = ipEndPoint.Address;
			LocalPort = ipEndPoint.Port;
		}
	}
}
