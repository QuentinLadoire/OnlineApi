using System.Net;
using System.Net.Sockets;

public class Client
{
	public Socket Socket { get; private set; }
	public IPAddress IpAddress { get; private set; }
	public int Port { get; private set; }

	public Client(Socket socket)
	{
		this.Socket = socket;

		var ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
		IpAddress = ipEndPoint.Address;
		Port = ipEndPoint.Port;
	}
}
