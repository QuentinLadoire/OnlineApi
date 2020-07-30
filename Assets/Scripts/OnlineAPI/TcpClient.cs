using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TcpClient
{
    public LogCallback logCallback = (string log) => { };

    Socket socket = null;

    public TcpClient()
	{
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public Client GetClientInfo()
	{
        return new Client(socket);
	}

    public bool ConnectTo(IPAddress serverAddress, int serverPort)
	{
        socket.Connect(serverAddress, serverPort);

        return socket.Connected;
	}

    public void SendMsg()
	{
        socket.Send(Encoding.UTF8.GetBytes("SendMessage"));
	}

    public void Update()
	{
        
    }

    public void Close()
	{
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }
}
