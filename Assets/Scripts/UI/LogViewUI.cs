using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogViewUI : MonoBehaviour
{
    [SerializeField] GameObject logPrefab = null;

	List<LogUI> logs = new List<LogUI>();

	public void AddLog(string log)
	{
		var newLogUI = Instantiate(logPrefab).GetComponent<LogUI>();
		newLogUI.transform.SetParent(logPrefab.transform.parent, false);
		newLogUI.gameObject.SetActive(true);

		newLogUI.SetLog(log);

		newLogUI.transform.localPosition = logPrefab.transform.localPosition + new Vector3(0.0f, -(newLogUI.transform as RectTransform).rect.height * logs.Count, 0.0f);

		logs.Add(newLogUI);
	}

	private void Start()
	{
		if (logPrefab != null) logPrefab.SetActive(false);
	}
}
