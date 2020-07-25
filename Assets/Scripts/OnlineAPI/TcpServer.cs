using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public delegate void DequeueCallback(string msg);
public delegate void ConnectionCallback(IPEndPoint[] ipEndPoint);

public class TcpServer
{
	public IPAddress Address { get; private set; }
	public int Port { get; private set; }

	public DequeueCallback dequeueCallBack = (string msg) => { };
	public ConnectionCallback connectionCallBack = (IPEndPoint[] ipEndPoint) => { };

	Socket socket = null;
	Thread threadConnection = null;

	List<Socket> connectedSockets = new List<Socket>();
	readonly object connectedSocketsLock = new object();

	Queue<string> logQueue = new Queue<string>();
	readonly object logQueueLock = new object();

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
				lock (connectedSocketsLock)
				{
					connectedSockets.Add(acceptedSocket);
				}

				lock (logQueueLock)
				{
					logQueue.Enqueue("New Connection");
				}
			}
		}
	}
	string Dequeue()
	{
		lock (logQueueLock)
		{
			if (logQueue.Count > 0)
			{
				return logQueue.Dequeue();
			}
		}

		return null;
	}
	IPEndPoint[] GetConnectedClient()
	{
		IPEndPoint[] ipEndpoints = null;

		lock(connectedSocketsLock)
		{
			ipEndpoints = new IPEndPoint[connectedSockets.Count];

			for (int i = 0; i < connectedSockets.Count; i++)
			{
				ipEndpoints[i] = connectedSockets[i].RemoteEndPoint as IPEndPoint;
			}
		}

		return ipEndpoints;
	}

	public void Start()
	{
		socket.Bind(new IPEndPoint(Address, Port));
		socket.Listen(10);
		
		threadConnection = new Thread(new ThreadStart(AcceptConnection));
		threadConnection.Start();

		lock (logQueueLock)
		{
			logQueue.Enqueue("Start Server");
		}
	}

	public void Update()
	{
		var msg = Dequeue();
		if (msg != null)
		{
			dequeueCallBack(msg);

			if (msg == "New Connection")
			{
				connectionCallBack(GetConnectedClient());
			}
		}
	}

	public void Close()
	{
		threadConnection.Abort();

		socket.Close();
	}
}
