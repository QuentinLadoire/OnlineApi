using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientListUI : MonoBehaviour
{
    [SerializeField] GameObject clientLabelPrefab = null;

	List<Text> clientLabels = new List<Text>();

	void UpdatePosition()
	{
		for (int i = 0; i < clientLabels.Count; i++)
		{
			clientLabels[i].transform.position = clientLabelPrefab.transform.position + new Vector3(0.0f, 60.0f * i, 0.0f);
		}
	}

	public void AddClient(string label)
	{
		var tmp = Instantiate(clientLabelPrefab).GetComponent<Text>();
		tmp.transform.SetParent(clientLabelPrefab.transform.parent, false);
		tmp.gameObject.SetActive(true);
		tmp.text = label;

		clientLabels.Add(tmp);

		UpdatePosition();
	}
	public void RemoveClient(string label)
	{
		var toRemove = clientLabels.Find(item => item.text == label);
		if (toRemove != null)
		{
			clientLabels.Remove(toRemove);
			Destroy(toRemove);

			UpdatePosition();
		}
	}
	public bool Contains(string label)
	{
		return clientLabels.Exists(item => item.text == label);
	}
	public void Clear()
	{
		foreach (var clientLabel in clientLabels)
		{
			Destroy(clientLabel.gameObject);
		}

		clientLabels.Clear();
	}

	private void Awake()
	{
		if (clientLabelPrefab != null) clientLabelPrefab.SetActive(false);
	}
}
