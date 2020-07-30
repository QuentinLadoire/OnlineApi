using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

public class TcpServer
{
	public IPAddress Address { get; private set; }
	public int Port { get; private set; }

	public LogCallback logCallback = (string log) => { };

	public ConnectionCallback connectionCallBack = (Client client) => { };
	public ConnectionCallback disConnectionCallBack = (Client client) => { };
	public MsgCallBack receiveMsg = (Message msg) => { };

	int checkConnectionTimeout = 1000;

	Socket socket = null;

	volatile bool shutdown = false;

	Thread threadConnection = null;
	Thread threadCheckConnection = null;
	Thread threadReceiveMsg = null;

	Queue<Client> newConnectedClientQueue = new Queue<Client>();
	readonly object newConnectedClientQueueLock = new object();

	Queue<Client> clientToRemoveQueue = new Queue<Client>();
	readonly object clientToRemoveQueueLock = new object();

	List<Client> connectedClients = new List<Client>();
	readonly object connectedClientsLock = new object();

	Queue<Message> msgQueue = new Queue<Message>();
	readonly object msgQueueLock = new object();


	public TcpServer(int port = 8000)
	{
		Address = OnlineUtility.GetLocalIPV4Address();
		Port = port;

		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}

	void AcceptConnection()
	{
		while (!shutdown)
		{
			var acceptedSocket = socket.Accept();
			if (acceptedSocket != null)
			{
				var newClient = new Client(acceptedSocket);
				lock (connectedClientsLock)
				{
					connectedClients.Add(newClient);
				}
				lock (newConnectedClientQueueLock)
				{
					newConnectedClientQueue.Enqueue(newClient);
				}
			}
		}
	}

	List<Socket> IsConnectedSockets(List<Socket> socketToTest)
	{
		if (connectedClients.Count > 0)
		{
			Socket.Select(socketToTest, null, null, 1000);
		}

		return socketToTest;
	}

	void CheckSocketConnection()
	{
		while (!shutdown)
		{
			lock (connectedClientsLock)
			{
				lock (clientToRemoveQueueLock)
				{
					var disconnectedSockets = IsConnectedSockets(connectedClients.Select(item => item.Socket).ToList());
					foreach (var disconnectedSocket in disconnectedSockets)
					{
						var clientToRemove = connectedClients.Find(item => item.Socket == disconnectedSocket);
						connectedClients.Remove(clientToRemove);
						clientToRemoveQueue.Enqueue(clientToRemove);
					}
				}
			}

			Thread.Sleep(checkConnectionTimeout);
		}
	}

	void ReceiveMsg()
	{
		byte[] buffer = new byte[256];
		while (!shutdown)
		{
			lock (connectedClientsLock)
			{
				if (connectedClients.Count > 0)
				{
					int byteCount = connectedClients[0].Socket.Receive(buffer);
					if (byteCount > 0)
					{
						lock (msgQueueLock)
						{
							msgQueue.Enqueue(new Message(buffer, byteCount));
						}
					}
				}
			}
		}
	}

	public Client GetClientInfo()
	{
		return new Client(socket);
	}

	public void Start()
	{
		socket.Bind(new IPEndPoint(Address, Port));
		socket.Listen(10);
		
		threadConnection = new Thread(new ThreadStart(AcceptConnection));
		threadConnection.Start();
		//threadConnection.Join();

		threadCheckConnection = new Thread(new ThreadStart(CheckSocketConnection));
		threadCheckConnection.Start();
		//threadCheckConnection.Join();

		threadReceiveMsg = new Thread(new ThreadStart(ReceiveMsg));
		threadReceiveMsg.Start();
		//threadReceiveMsg.Join();

		logCallback("Server Start");
	}

	public void Update()
	{
		if (Monitor.TryEnter(newConnectedClientQueueLock))
		{
			if (newConnectedClientQueue.Count > 0)
			{
				connectionCallBack(newConnectedClientQueue.Dequeue());
			}

			Monitor.Exit(newConnectedClientQueueLock);
		}

		if (Monitor.TryEnter(clientToRemoveQueueLock))
		{
			if (clientToRemoveQueue.Count > 0)
			{
				disConnectionCallBack(clientToRemoveQueue.Dequeue());
			}

			Monitor.Exit(clientToRemoveQueueLock);
		}

		if (Monitor.TryEnter(msgQueueLock))
		{
			if (msgQueue.Count > 0)
			{
				var msg = msgQueue.Dequeue();
				receiveMsg(msg);

				logCallback(Encoding.UTF8.GetString(msg.Bytes));
			}

			Monitor.Exit(msgQueueLock);
		}
	}

	public void Close()
	{
		shutdown = true;

		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
	}
}
