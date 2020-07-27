using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientListUI : MonoBehaviour
{
    [SerializeField] GameObject clientInfoPrefab = null;

	List<ClientInfoUI> clientInfos = new List<ClientInfoUI>();

	void UpdatePosition()
	{
		for (int i = 0; i < clientInfos.Count; i++)
		{
			clientInfos[i].transform.localPosition = clientInfoPrefab.transform.localPosition + new Vector3(0.0f, -80.0f * i, 0.0f);
		}
	}

	public void AddClient(string ip, string port)
	{
		var clientInfo = Instantiate(clientInfoPrefab).GetComponent<ClientInfoUI>();

		clientInfo.transform.SetParent(clientInfoPrefab.transform.parent, false);
		clientInfo.gameObject.SetActive(true);

		clientInfo.SetIpAndPort(ip, port);

		clientInfos.Add(clientInfo);

		UpdatePosition();
	}
	public void RemoveClient(string ip , string port)
	{
		var toRemove = clientInfos.Find(item => item.Ip == ip && item.Port == port);
		if (toRemove != null)
		{
			clientInfos.Remove(toRemove);
			Destroy(toRemove);

			UpdatePosition();
		}
	}
	public bool Contains(string ip, string port)
	{
		return clientInfos.Exists(item => item.Ip == ip && item.Port == port);
	}
	public void Clear()
	{
		foreach (var clientLabel in clientInfos)
		{
			Destroy(clientLabel.gameObject);
		}

		clientInfos.Clear();
	}

	private void Awake()
	{
		if (clientInfoPrefab != null) clientInfoPrefab.SetActive(false);
	}
}
