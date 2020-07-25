using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class TcpClient
{
    Socket m_socket = null;

    public TcpClient()
	{
        m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void ConnectTo(IPAddress serverAddress, int serverPort)
	{
        m_socket.Connect(serverAddress, serverPort);
	}
}
