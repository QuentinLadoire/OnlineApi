using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

public class TcpServer
{
	static TcpServer instance = null;

	public IPAddress Address { get; private set; }
	public int Port { get; private set; }

	public LogCallback logCallback = (string log) => { };

	public ConnectionCallback connectionCallBack = (Client client) => { };
	public ConnectionCallback disConnectionCallBack = (Client client) => { };
	public MsgCallBack receiveMsgCallBack = (byte[] msg) => { };
	public MsgCallBack sendingMsgCallBack = (byte[] msg) => { };

	Socket socket = null;

	int idGenerator = 0;

	volatile bool shutdown = false;

	Thread threadConnection = null;
	Thread threadStopRequested = null;

	Queue<Client> newConnectedClientQueue = new Queue<Client>();
	readonly object newConnectedClientQueueLock = new object();

	Queue<Client> clientToRemoveQueue = new Queue<Client>();
	readonly object clientToRemoveQueueLock = new object();

	List<Client> connectedClients = new List<Client>();
	readonly object connectedClientsLock = new object();

	Queue<Message> receiveMsgQueue = new Queue<Message>();
	readonly object receiveMsgQueueLock = new object();

	Queue<byte[]> sendMsgQueue = new Queue<byte[]>();
	readonly object sendMsgQueueLock = new object();

	Queue<string> logQueue = new Queue<string>();
	readonly object logQueueLock = new object();

	public static void Log(string log)
	{
		lock (instance.logQueueLock)
		{
			instance.logQueue.Enqueue(log);
		}
	}
	public static void AddSendingMsgCallBack(MsgCallBack msgCallBack)
	{
		instance.sendingMsgCallBack += msgCallBack;
	}
	public static void RemoveSendindMsgCallBack(MsgCallBack msgCallBack)
	{
		instance.sendingMsgCallBack -= msgCallBack;
	}

	public TcpServer(int port = 8000)
	{
		Address = OnlineUtility.GetLocalIPV4Address();
		Port = port;

		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		instance = this;
	}

	public SocketInfo GetSocketInfo()
	{
		return new SocketInfo(socket);
	}

	int GenerateId()
	{
		var id = idGenerator;
		idGenerator++;

		return id;
	}

	void AcceptConnection()
	{
		while (!shutdown)
		{
			try
			{
				var acceptedSocket = socket.Accept();
				if (acceptedSocket != null)
				{
					var newClient = new Client(GenerateId(), acceptedSocket, ReceiveMsg, CheckSocketConnection);
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
			catch
			{
				Log("Connexion Abort");
			}
		}
	}
	void CheckSocketConnection(Client client)
	{
		while (!shutdown && !client.Shutdown)
		{
			if (client.Socket != null)
			{
				bool tmp = false;
				lock (connectedClientsLock)
				{
					tmp = connectedClients.Contains(client);
				}

				if (tmp && !OnlineUtility.IsConnectedSocket(client.Socket))
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
	void ReceiveMsg(Client client)
	{
		byte[] buffer = new byte[256];
		while (!shutdown && !client.Shutdown)
		{
			try
			{
				if (client.Socket != null)
				{
					int byteCount = client.Socket.Receive(buffer);
					if (byteCount > 0)
					{
						lock (receiveMsgQueueLock)
						{
							receiveMsgQueue.Enqueue(new Message(client.Id, buffer, byteCount));
						}
					}
				}
			}
			catch
			{
				Log("Receive Abort");
			}
		}
	}
	void AddMsgToSendingQueue(object bytesObject)
	{
		lock (sendMsgQueueLock)
		{
			var bytes = bytesObject as byte[];

			sendMsgQueue.Enqueue(bytes);
		}
	}
	void StopRequested()
	{
		threadConnection.Join();

		lock (connectedClientsLock)
		{
			foreach (var client in connectedClients)
			{
				client.CloseConnection();
			}

			connectedClients.Clear();
		}
		lock (clientToRemoveQueueLock)
		{
			foreach (var client in clientToRemoveQueue)
			{
				client.CloseConnection();
			}

			clientToRemoveQueue.Clear();
		}
		lock(newConnectedClientQueueLock)
		{
			foreach (var client in newConnectedClientQueue)
			{
				client.CloseConnection();
			}

			newConnectedClientQueue.Clear();
		}

		Log("Server Stop");
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

				logCallback("Client " + clientToRemove.Id + " Disconnected");

				clientToRemove.CloseConnection();
		
				disConnectionCallBack(clientToRemove);
			}
		
			Monitor.Exit(clientToRemoveQueueLock);
		}
	}
	void ProcessReceiveMsg()
	{
		if (Monitor.TryEnter(receiveMsgQueueLock))
		{
			if (receiveMsgQueue.Count > 0)
			{
				var msg = receiveMsgQueue.Dequeue();
				receiveMsgCallBack(msg.Bytes);

				logCallback("Client " + msg.Id + " : " + Encoding.UTF8.GetString(msg.Bytes));
			}

			Monitor.Exit(receiveMsgQueueLock);
		}
	}
	void ProcessSendingMsg()
	{
		if (Monitor.TryEnter(sendMsgQueueLock))
		{
			if (sendMsgQueue.Count > 0)
			{
				sendingMsgCallBack(sendMsgQueue.Dequeue());
			}

			Monitor.Exit(sendMsgQueueLock);
		}
	}
	void ProcessLog()
	{
		if (Monitor.TryEnter(logQueueLock))
		{
			if (logQueue.Count > 0)
			{
				logCallback(logQueue.Dequeue());
			}

			Monitor.Exit(logQueueLock);
		}
	}
	
	public void SendMsg(byte[] bytes)
	{
		ThreadPool.QueueUserWorkItem(AddMsgToSendingQueue, bytes);
	}

	public void Start()
	{
		socket.Bind(new IPEndPoint(Address, Port));
		socket.Listen(10);
		
		threadConnection = new Thread(new ThreadStart(AcceptConnection));
		threadConnection.Start();

		shutdown = false;

		logCallback("Server Start");
	}
	public void Update()
	{
		if (!shutdown)
		{
			ProcessNewConnectedClient();

			ProcessDisConnectedClient();

			ProcessReceiveMsg();

			ProcessSendingMsg();
		}

		ProcessLog();
	}
	public void Close()
	{
		shutdown = true;

		socket.Close();

		threadStopRequested = new Thread(new ThreadStart(StopRequested));
		threadStopRequested.Start();
	}
}
