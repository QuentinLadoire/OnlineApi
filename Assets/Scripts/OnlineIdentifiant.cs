using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineIdentifiant : MonoBehaviour
{
    public int Id { get; private set; }

    public void Set(int id)
	{
		Id = id;
	}
}
