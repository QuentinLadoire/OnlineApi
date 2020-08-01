
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class Client
{
	public int Id { get; private set; }

	public Socket Socket { get; private set; }

	public SocketInfo SocketInfo { get; private set; }

	public Thread ThreadReceiveMsg { get; private set; }
	public Thread ThreadCheckConnection { get; private set; }

	Thread threadRequestedClose = null;

	volatile bool shutdown = false;
	public bool Shutdown { get => shutdown; }

	void RequestedClose()
	{
		ThreadCheckConnection.Join();
		ThreadReceiveMsg.Join();

		Socket.Shutdown(SocketShutdown.Both);
		Socket.Close();
	}

	public void CloseConnection()
	{
		shutdown = true;

		threadRequestedClose = new Thread(new ThreadStart(RequestedClose));
		threadRequestedClose.Start();
	}

	public Client(int id, Socket socket, ReceiveMsgThread receiveMsgThread, CheckConnectionThread checkConnectionThread)
	{
		Id = id;

		shutdown = false;

		Socket = socket;

		SocketInfo = new SocketInfo(Socket);

		ThreadReceiveMsg = new Thread(new ThreadStart(() => { receiveMsgThread(this); }));
		ThreadReceiveMsg.Start();

		ThreadCheckConnection = new Thread(new ThreadStart(() => { checkConnectionThread(this); }));
		ThreadCheckConnection.Start();
	}
}
