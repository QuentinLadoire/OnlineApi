using System.Linq;
using System.Net.Sockets;
using System.Threading;

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

	void SendMsg(MsgInfo msgInfo)
	{
		if (msgInfo.ClientIds == null || msgInfo.ClientIds.Contains(Id))
		{
			Socket.Send(msgInfo.Bytes);
		}
	}

	void RequestedClose()
	{
		ThreadCheckConnection.Join();

		try
		{
			Socket.Shutdown(SocketShutdown.Both);
		}
		finally
		{
			Socket.Close();
		}
	}

	public void CloseConnection()
	{
		shutdown = true;

		TcpServer.RemoveSendindMsgCallBack(SendMsg);

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

		TcpServer.AddSendingMsgCallBack(SendMsg);
	}
}
