
using System.Net.Sockets;

public delegate void LogCallback(string log);
public delegate void ConnectionCallback(Client client);
public delegate void MsgCallBack(Message msg);

public delegate void ReceiveMsgThread(Client client);
public delegate void CheckConnectionThread(Client client);
