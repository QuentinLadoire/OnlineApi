using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TcpClient
{
	static TcpClient instance = null;

    public LogCallback logCallback = (string log) => { };

	public MsgCallBack receiveMsgCallBack = (byte[] msg) => { };

	public bool IsConnected { get => isConnected; }

    Socket socket = null;

	volatile bool shutdown = false;
	volatile bool isConnected = false;

	Thread threadReceiveMsg = null;
	Thread threadCheckConnection = null;

	Queue<byte[]> receiveMsgQueue = new Queue<byte[]>();
	readonly object receiveMsgQueueLock = new object();

	Queue<string> logQueue = new Queue<string>();
	readonly object logQueueLock = new object();

	public static void Log(string log)
	{
		lock (instance.logQueueLock)
		{
			instance.logQueue.Enqueue(log);
		}
	}

    public TcpClient()
	{
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		instance = this;
    }

	public SocketInfo GetSocketInfo()
	{
        return new SocketInfo(socket);
	}

	void CheckSocketConnection()
	{
		while (!shutdown)
		{
			if (socket != null)
			{
				isConnected = OnlineUtility.IsConnectedSocket(socket);
			}
		}
	}
	void ReceiveMsg()
	{
		byte[] buffer = new byte[256];
		while (!shutdown)
		{
			try
			{
				if (socket != null)
				{
					int byteCount = socket.Receive(buffer);
					if (byteCount > 0)
					{
						lock (receiveMsgQueueLock)
						{
							var msg = new byte[byteCount];
							Array.Copy(buffer, msg, byteCount);

							receiveMsgQueue.Enqueue(msg);
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

	void ProcessDisconnection()
	{
		if (!isConnected)
		{
			logCallback("Server disconnected");
		}
	}
	void ProcessMsg()
	{
		if (Monitor.TryEnter(receiveMsgQueueLock))
		{
			if (receiveMsgQueue.Count > 0)
			{
				var msg = receiveMsgQueue.Dequeue();

				receiveMsgCallBack(msg);

				logCallback("Server : " + Encoding.UTF8.GetString(msg));
			}

			Monitor.Exit(receiveMsgQueueLock);
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
        socket.Send(bytes);
	}

    public bool ConnectTo(IPAddress serverAddress, int serverPort)
	{
        socket.Connect(serverAddress, serverPort);
		if (socket.Connected)
		{
			isConnected = true;

			threadReceiveMsg = new Thread(new ThreadStart(ReceiveMsg));
			threadReceiveMsg.Start();

			threadCheckConnection = new Thread(new ThreadStart(CheckSocketConnection));
			threadCheckConnection.Start();
		}

        return socket.Connected;
	}
    public void Update()
	{
        if (!shutdown)
		{
			ProcessDisconnection();

			ProcessMsg();
		}

		ProcessLog();
	}
    public void Close()
	{
		shutdown = true;

		try
		{
			socket.Shutdown(SocketShutdown.Both);
		}
		finally
		{
			socket.Close();
		}
    }
}
