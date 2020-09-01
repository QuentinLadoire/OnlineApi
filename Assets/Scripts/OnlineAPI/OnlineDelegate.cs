
using System.Net.Sockets;

public delegate void LogCallback(string log);
public delegate void ConnectionCallback(Client client);
public delegate void MsgCallBack(byte[] msg);
public delegate void SendingMsgCallBack(MsgInfo msgInfo);

public delegate void ReceiveMsgThread(Client client);
public delegate void CheckConnectionThread(Client client);

public delegate void MsgCallBackListener(MsgCallBack msgCallBack);
