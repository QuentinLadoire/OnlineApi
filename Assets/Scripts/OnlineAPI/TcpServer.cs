using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class TcpServer
{
	public IPAddress Address { get; private set; }
	public int Port { get; private set; }

	public LogCallback logCallback = (string log) => { };

	public ConnectionCallback connectionCallBack = (Client client) => { };
	public ConnectionCallback disConnectionCallBack = (Client client) => { };

	Socket socket = null;
	Thread threadConnection = null;

	Queue<Client> newConnectedClientQueue = new Queue<Client>();
	readonly object newConnectedClientQueueLock = new object();

	List<Client> connectedClients = new List<Client>();
	readonly object connectedClientsLock = new object();

	public TcpServer(int port = 8000)
	{
		Address = OnlineUtility.GetLocalIPV4Address();
		Port = port;

		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}

	void AcceptConnection()
	{
		while (true)
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
		lock (newConnectedClientQueueLock)
		{
			if (newConnectedClientQueue.Count > 0)
			{
				connectionCallBack(newConnectedClientQueue.Dequeue());
			}
		}

		lock (connectedClientsLock)
		{
			foreach (var client in connectedClients)
			{
				logCallback("Client Socket Connected : " + client.Socket.Connected.ToString() + " - " + "Client Socket Available : " + client.Socket.Available);
			}
		}
	}

	public void Close()
	{
		threadConnection.Abort();

		socket.Shutdown(SocketShutdown.Both);
		socket.Close();
	}
}
