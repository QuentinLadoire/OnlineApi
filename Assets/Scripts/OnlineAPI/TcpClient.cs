using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class TcpClient
{
    public LogCallback logCallback = (string log) => { };

    Socket socket = null;

    public TcpClient()
	{
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public bool ConnectTo(IPAddress serverAddress, int serverPort)
	{
        socket.Connect(serverAddress, serverPort);
        return socket.Connected;
	}
    
    public void Update()
	{
        logCallback("Connected Socket : " + socket.Connected);
    }

    public void Close()
	{
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
}
