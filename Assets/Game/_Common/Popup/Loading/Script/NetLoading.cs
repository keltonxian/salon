using UnityEngine;
using PureMVC.Core;

public class NetLoading : Base
{
	void Awake () {
		gameObject.GetComponent<Canvas> ().worldCamera = Camera.main;
	}

	public void Dispose(){
		Destroy(gameObject);
	}
}
