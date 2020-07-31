using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

public class TcpServer
{
	public IPAddress Address { get; private set; }
	public int Port { get; private set; }

	public LogCallback logCallback = (string log) => { };

	public ConnectionCallback connectionCallBack = (Client client) => { };
	public ConnectionCallback disConnectionCallBack = (Client client) => { };
	public MsgCallBack receiveMsg = (Message msg) => { };

	int idGenerator = 0;

	Socket socket = null;

	volatile bool shutdown = false;

	Thread threadStopRequested = null;
	Thread threadConnection = null;

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

	public int GenerateId()
	{
		var id = idGenerator;
		idGenerator++;

		return id;
	}

	bool IsConnectedSocket(Socket socket)
	{
		if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
		{
			return false;
		}

		return true;
	}

	public SocketInfo GetSocketInfo()
	{
		return new SocketInfo(socket);
	}

	void AcceptConnection()
	{
		while (!shutdown)
		{
			var acceptedSocket = socket.Accept();
			if (acceptedSocket != null)
			{
				var newClient = new Client(GenerateId(), acceptedSocket, new ReceiveMsgThread(ReceiveMsg), new CheckConnectionThread(CheckSocketConnection));
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
	void CheckSocketConnection(Client client)
	{
		while (!shutdown || !client.Shutdown)
		{
			if (client.Socket != null)
			{
				lock (connectedClientsLock)
				{
					if (connectedClients.Contains(client))
					{
						if (!IsConnectedSocket(client.Socket))
						{
							connectedClients.Remove(client);

							lock (clientToRemoveQueueLock)
							{
								clientToRemoveQueue.Enqueue(client);
							}
						}
					}
				}
			}
		}
	}
	void ReceiveMsg(Client client)
	{
		byte[] buffer = new byte[256];
		while (!shutdown || !client.Shutdown)
		{
			if (client.Socket != null)
			{
				int byteCount = client.Socket.Receive(buffer);
				if (byteCount > 0)
				{
					lock (msgQueueLock)
					{
						msgQueue.Enqueue(new Message(client.Id, buffer, byteCount));
					}
				}
			}
		}
	}

	void ProcessNewConnectedClient()
	{
		if (Monitor.TryEnter(newConnectedClientQueueLock))
		{
			if (newConnectedClientQueue.Count > 0)
			{
				connectionCallBack(newConnectedClientQueue.Dequeue());
			}

			Monitor.Exit(newConnectedClientQueueLock);
		}
	}
	void ProcessDisConnectedClient()
	{
		if (Monitor.TryEnter(clientToRemoveQueueLock))
		{
			if (clientToRemoveQueue.Count > 0)
			{
				var clientToRemove = clientToRemoveQueue.Dequeue();
				clientToRemove.CloseConnection();

				disConnectionCallBack(clientToRemove);
			}

			Monitor.Exit(clientToRemoveQueueLock);
		}
	}
	void ProcessMsg()
	{
		if (Monitor.TryEnter(msgQueueLock))
		{
			if (msgQueue.Count > 0)
			{
				var msg = msgQueue.Dequeue();
				receiveMsg(msg);

				logCallback("Client " + msg.Id + " : " + Encoding.UTF8.GetString(msg.Bytes));
			}

			Monitor.Exit(msgQueueLock);
		}
	}
	
	void StopRequested()
	{
		lock (connectedClientsLock)
		{
			foreach (var client in connectedClients)
			{
				client.CloseConnection();
			}
		}
		lock (clientToRemoveQueueLock)
		{
			foreach (var client in clientToRemoveQueue)
			{
				client.CloseConnection();
			}
		}
		lock(newConnectedClientQueueLock)
		{
			foreach (var client in newConnectedClientQueue)
			{
				client.CloseConnection();
			}
		}
	}

	public void Start()
	{
		socket.Bind(new IPEndPoint(Address, Port));
		socket.Listen(10);
		
		threadConnection = new Thread(new ThreadStart(AcceptConnection));
		threadConnection.Start();

		logCallback("Server Start");
	}

	public void Update()
	{
		if (shutdown) return;

		ProcessNewConnectedClient();

		ProcessDisConnectedClient();

		ProcessMsg();
	}

	public void Close()
	{
		shutdown = true;

		socket.Shutdown(SocketShutdown.Both);
		socket.Close();

		threadStopRequested = new Thread(new ThreadStart(StopRequested));
		threadStopRequested.Start();
	}
}
