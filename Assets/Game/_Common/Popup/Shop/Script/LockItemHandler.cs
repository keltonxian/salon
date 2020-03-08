using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Core;
using PureMVC.Manager;

public class LockItemHandler : Base {

	public GameObject[] _arrayCharactersLockItem;
	public GameObject[] _arrayAccessoriesLockItem;
	public GameObject[] _arrayDecorationsLockItem;
	public GameObject[] _arrayShop = null;

	private RectTransform _sceneLock = null;

	public delegate void UnlockAnimCallback (GameObject lockItem);
	public delegate void UnlockAnimCallbackWithItem (GameObject[] arrayLockItem=null);
	private float _animTime = 0.5f;

	// Use this for initialization
	void Start () {
		CheckHideShop ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void IapUnlock (LockManager.IAP_TYPE iapType, UnlockAnimCallbackWithItem callback=null) {
		if (iapType == LockManager.IAP_TYPE.FULL) {
			if (LockManager.IsFullUnlocked ()) {
				bool hasCallback = false;
				if (null != _arrayCharactersLockItem && _arrayCharactersLockItem.Length > 0) {
					UnlockItemArray (_arrayCharactersLockItem, callback);
					hasCallback = true;
				}
				if (null != _arrayAccessoriesLockItem && _arrayAccessoriesLockItem.Length > 0) {
					UnlockItemArray (_arrayAccessoriesLockItem, callback);
					hasCallback = true;
				}
				if (null != _arrayDecorationsLockItem && _arrayDecorationsLockItem.Length > 0) {
					UnlockDecorationItemArray (_arrayDecorationsLockItem, callback);
					hasCallback = true;
				}
				if (false == hasCallback) {
					callback ();
				}
			}
		}
		CheckHideShop ();
	}

	public bool IsAllUnlock () {
        return LockManager.IsFullUnlocked();
	}

	private void CheckHideShop () {
		if (0 == _arrayShop.Length) {
			return;
		}
		bool isHide = IsAllUnlock ();
		if (false == isHide) {
			return;
		}
		for (int i = 0; i < _arrayShop.Length; i++) {
			_arrayShop[i].SetActive (false);
		}
	}

	private void UnlockDecorationItemArray (GameObject[] arrayItem, UnlockAnimCallbackWithItem callback=null) {
		for (int i = 0; i < arrayItem.Length; i++) {
			GameObject item = arrayItem [i];
			if (null == item) {
				continue;
			}
			if (item.transform.childCount == 0) {
				continue;
			}
			SidebarItemKit itemLock = item.GetComponent<SidebarItemKit> ();
			itemLock.UnlockAll ();
		}
		float waitTime = _animTime;
		if (0 == arrayItem.Length) {
			waitTime = 0f;
		}
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval (waitTime);
		sequence.AppendCallback (() => {
			if (null != callback) {
				callback (arrayItem);
			}
		});
	}

	private void UnlockItemArray (GameObject[] arrayItem, UnlockAnimCallbackWithItem callback=null) {
		for (int i = 0; i < arrayItem.Length; i++) {
			GameObject item = arrayItem [i];
			if (null == item) {
				continue;
			}
			Transform itemLock = item.transform.Find ("ItemLock");
			if (null != itemLock) {
				Sequence seq = DOTween.Sequence ();
				seq.Append (itemLock.DOScale (new Vector3 (0f, 0f, 1f), _animTime).SetEase (Ease.InBack));
				seq.OnComplete (() => {
					Destroy (itemLock.gameObject);
				});
				if (null != item.GetComponent<LockItemKit> ()) {
					item.GetComponent<LockItemKit> ().UnlockVideoLock (true);
				}
			}
		}
		float waitTime = _animTime;
		if (0 == arrayItem.Length) {
			waitTime = 0f;
		}
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval (waitTime);
		sequence.AppendCallback (() => {
			if (null != callback) {
				callback (arrayItem);
			}
		});
	}

	private void UnlockCharacters (UnlockAnimCallbackWithItem callback=null) {
		UnlockItemArray (_arrayCharactersLockItem, callback);	
	}

	private void UnlockDecorations (UnlockAnimCallbackWithItem callback=null) {
		UnlockDecorationItemArray (_arrayDecorationsLockItem, callback);	
	}

	private void UnlockAccessories (UnlockAnimCallbackWithItem callback=null) {
		UnlockItemArray (_arrayAccessoriesLockItem, callback);	
	}

	public void VideoUnlockScene (UnlockAnimCallback callback=null) {
		if (null == _sceneLock) {
			return;
		}
		VideoUnlock (_sceneLock.gameObject, callback);
	}

	private void UnlockNoAds (UnlockAnimCallbackWithItem callback=null) {
		Sequence sequence = DOTween.Sequence();
		sequence.AppendInterval (0.1f);
		sequence.AppendCallback (() => {
			if (null != callback) {
				callback ();
			}
		});
	}

	public void ActiveSceneLock (RectTransform sceneLockParent, Callback.CallbackV callback = null) {
		if (null != _sceneLock) {
			return;
		}
		RectTransform sceneLock = Instantiate (Resources.Load<RectTransform> ("SceneLock"));
		_sceneLock = sceneLock;
		sceneLock.SetParent (sceneLockParent);
		sceneLock.localPosition = Vector3.zero;
		sceneLock.localScale = Vector3.one;
		sceneLock.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Left, 0f, 0f);
		sceneLock.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Right, 0f, 0f);
		sceneLock.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Top, 0f, 0f);
		sceneLock.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Bottom, 0f, 0f);
		sceneLock.anchorMin = Vector2.zero;
		sceneLock.anchorMax = Vector2.one;
		sceneLock.gameObject.SetActive (true);
		sceneLock.Find ("ItemLockImage").GetComponent<UIButton> ().OnAnimDone.AddListener (() => {
			if (null != callback) {
				callback ();
			}
		});
	}

	public RectTransform GetSceneLock () {
		return _sceneLock;
	}

	public void VideoUnlock (GameObject itemLock, UnlockAnimCallback callback = null, bool hasParticle = true, bool hasMusic = true) {
		if (null == itemLock) {
			return;
		}
		RectTransform itemLockImage = itemLock.transform.Find ("ItemLockImage").GetComponent<RectTransform> ();
		if (null == itemLockImage) {
			return;
		}
		//Particle2DSystem particle = null;
		//if (hasParticle) {
		//	particle = Instantiate (Resources.Load<Particle2DSystem> ("UnlockParticle"));
		//	particle.transform.SetParent (itemLock.transform);
		//	particle.transform.localPosition = Vector3.zero;
		//	particle.transform.localScale = itemLockImage.localScale;
		//}
		if (hasMusic) {
            UIManager.PlaySoundUnlock();
		}
		Sequence seq = DOTween.Sequence();
		seq.PrependInterval (0.5f);
		seq.Append (itemLockImage.DOScale (new Vector3 (0f, 0f, 1f), 1f).SetEase (Ease.InBack));
		seq.OnComplete (() => {
			//if (null != particle) {
			//	Destroy (particle.gameObject);
			//}
			Destroy (itemLockImage.gameObject);
			if (null != itemLock.GetComponent<Image> ()) {
				itemLock.GetComponent<Image> ().raycastTarget = false;
			}
			if (null != callback) {
				callback (itemLock);
			}
		});
	}

}
