using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogUI : MonoBehaviour
{
    [SerializeField] Text logLabel = null;

    public void SetLog(string log)
	{
		var dateTime = DateTime.Now;
		logLabel.text = "[" + dateTime.Hour + ":" + dateTime.Minute + ":" + dateTime.Second + "]" + " " + log;
	}
}
