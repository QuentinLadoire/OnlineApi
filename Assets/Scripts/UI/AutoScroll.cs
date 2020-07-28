using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoScroll : MonoBehaviour
{
	[SerializeField] Toggle toggle = null;

    ScrollRect scrollRect = null;
	bool active = false;

	private void Start()
	{
		scrollRect = GetComponent<ScrollRect>();

		toggle.onValueChanged.AddListener((bool value) =>
		{
			active = value;
		});
	}

	private void Update()
	{
		if (active)
		{
			scrollRect.verticalScrollbar.value = 0.0f;
		}
	}
}
