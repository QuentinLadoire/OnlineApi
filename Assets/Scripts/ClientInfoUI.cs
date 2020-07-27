using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientInfoUI : MonoBehaviour
{
    [SerializeField] Text ipLabel = null;
    [SerializeField] Text portLabel = null;

    public string Ip { get; private set; }
    public string Port { get; private set; }

    public void SetIp(string ip)
	{
        Ip = ip;
        ipLabel.text = "Ip : " + ip;
	}
    public void SetPort(string port)
	{
        Port = port;
        portLabel.text = "Port : " + port;
	}
    public void SetIpAndPort(string ip, string port)
	{
        SetIp(ip);
        SetPort(port);
	}
}
