using UnityEngine;
using UnityEngine.UI;

public class MsgBox : MonoBehaviour {

	public Text _text;

    void Awake()
    {
        gameObject.GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Use this for initialization
    void Start () {
		Invoke ("Dispose", 3f);
	}

	public void ShowMsg (string msg) {
		_text.text = msg;
	}

	public void Dispose(){
		Destroy(gameObject);
	}

}
