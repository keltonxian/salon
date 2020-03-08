using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Core;
using PureMVC.Manager;

[RequireComponent(typeof(RectTransform))]
public class LockItemKit : Base {

	public LockManager.IAP_TYPE _iapType = LockManager.IAP_TYPE.NONE;
	public string _lockKey = null;
	public Sprite _lockSprite = null;
	public Vector2 _lockOffset = Vector2.zero;
	public float _lockScale = 1f;
	public Vector2 _lockAreaSize = Vector2.zero;
	private GameObject _itemLock;
	private Callback.CallbackB _iapUnlockCallback;
	private Callback.CallbackB _videoUnlockCallback;
	private LockItemHandler _lockItemHandler;
	public bool _isAutoAddLock = true;

	private bool _isLockForTool = false;
	public bool IsLockForTool {
		get {
			return _isLockForTool;
		}
		set {
			_isLockForTool = value;
		}
	}

	void Awake () {
		_lockItemHandler = GameObject.Find ("UICanvas").GetComponent<LockItemHandler> ();
	}

	void Start () {
		if (!_isAutoAddLock) {
			return;
		}
		AddLock ((bool isUnlock) => {
			UnlockVideoLock (isUnlock);
		}, (bool isUnlock) => {
			UnlockVideoLock (isUnlock);
		});
		if (IsLock ()) {
			SetToolTouchEnabled (false);
		}
	}

	public void AddLock (Callback.CallbackB iapCallback = null, Callback.CallbackB videoCallback = null) {
		if (string.IsNullOrEmpty (_lockKey) || null == _lockSprite) {
			return;
		}
        if (LockManager.IsFullUnlocked())
        {
            return;
        }
		_iapUnlockCallback = iapCallback;
		_videoUnlockCallback = videoCallback;
		if (Vector2.zero == _lockAreaSize) {
			_lockAreaSize = this.GetComponent<RectTransform> ().sizeDelta;
		}
		GameObject itemLock = new GameObject ("ItemLock");
		itemLock.AddComponent<Image> ();
		itemLock.AddComponent<UIButton> ();
		itemLock.GetComponent<RectTransform> ().SetParent (this.GetComponent<RectTransform> ());
		itemLock.transform.localPosition = Vector3.zero;
		itemLock.GetComponent<RectTransform> ().sizeDelta = _lockAreaSize;
		itemLock.GetComponent<RectTransform> ().localScale = Vector3.one;
		itemLock.GetComponent<RectTransform> ().localPosition = Vector3.zero;
		itemLock.GetComponent<RectTransform> ().anchoredPosition = Vector3.zero;
		itemLock.GetComponent<Image> ().color = new Color (0f, 0f, 0f, 0f);
		itemLock.GetComponent<UIButton> ().OnAnimDone.AddListener (Unlock);
		GameObject lockImage = new GameObject ("ItemLockImage");
		lockImage.AddComponent<Image> ();
		lockImage.GetComponent<RectTransform> ().SetParent (itemLock.GetComponent<RectTransform> ());
		lockImage.transform.localPosition = Vector3.zero;
		lockImage.GetComponent<RectTransform> ().localScale = new Vector3 (_lockScale, _lockScale, 1f);
		lockImage.GetComponent<RectTransform> ().anchoredPosition = _lockOffset;
		lockImage.GetComponent<Image> ().sprite = _lockSprite;
		lockImage.GetComponent<Image> ().SetNativeSize ();

		SetItemLock (itemLock);
	}

	private void SetItemLock (GameObject itemLock = null) {
		_itemLock = itemLock;
	}

	public void RemoveLock (bool hasParticle = true, bool hasMusic = true, Callback.CallbackV callback = null) {
		if (null == _itemLock) {
			return;
		}
		GameObject itemLock = _itemLock;
		SetItemLock ();

		_lockItemHandler.VideoUnlock (itemLock, (GameObject lockItem) => {
		}, hasParticle, hasMusic);
	}

	public bool IsLock () {
		return (_itemLock != null);
	}

//	private void AdsShowHandler (AdType obj) {
//		if (obj == AdType.AdTypeInterstitialAds) {
//			PluginManager.Instance.OnAdsShowHandler -= AdsShowHandler;
////			Time.timeScale = 0f;
//			return;
//		}
//	}

//	private void AdsRemoveHandler (AdType obj) {
//		if (obj == AdType.AdTypeInterstitialAds) {
//			PluginManager.Instance.OnAdsRemoveHandler -= AdsRemoveHandler;
////			PluginManager.Instance.OnAdsShowTimeoutHandler -= AdsShowTimeoutHandler;
////			Time.timeScale = 1f;
//			UnlockByInterstitialAds ();
//			return;
//		}
//	}
//
//	private void AdsShowTimeoutHandler (AdType obj) {
//		if (obj == AdType.AdTypeInterstitialAds) {
//			PluginManager.Instance.OnAdsRemoveHandler -= AdsRemoveHandler;
////			PluginManager.Instance.OnAdsShowTimeoutHandler -= AdsShowTimeoutHandler;
//			TipTimeout ();
//			return;
//		}
//	}
//
//	private void TipTimeout () {
//		PluginManager.Instance.internalSDk.popAlertDialog ("Unlocking requires an internet connection yet it seems like you are not connected to the internet. Please check your settings or try again later.");
//	}

	private void UnlockByInterstitialAds () {
		LockManager.UnlockItemByVideo (_lockKey);
		RemoveLock ();
		if (null != _videoUnlockCallback) {
			_videoUnlockCallback (true);
		}
	}

	private void OnUnlockedForFree () {
		LockManager.UnlockItemByVideo (_lockKey);
		UnlockByInterstitialAds ();
	}

	private void GetItForFree () {
		if (string.IsNullOrEmpty (_lockKey)) {
			return;
		}
		if (Application.isEditor) {
			OnUnlockedForFree ();
			return;
		}
		AdsManager.ShowRewardedAd ((bool isDone) => {
			if (true == isDone) {
				OnUnlockedForFree ();
			}
		});
	}

	public void Unlock () {
		if (!IsLock ()) {
			return;
		}
		if (Application.platform == RuntimePlatform.Android) {
			GetItForFree ();
		} else {
			UIManager.ShowMiniShop (
				_iapType,
				_lockKey,
				(LockManager.IAP_TYPE iapType) => {
					_lockItemHandler.IapUnlock (iapType, (GameObject[] arrayLockItem) => {
						_iapUnlockCallback (true);
					});
				},
				(bool isUnlocked) => {
					if (false == isUnlocked) { return; }
					UnlockByInterstitialAds ();
				},
				() => {
					// Loading loading = (Loading)Instantiate (Resources.Load<Loading> ("LoadingCustom"));
					// loading._nextSceneName = "SceneMap";
				}
			);
		}
	}

	private void SetToolTouchEnabled (bool isEnabled) {
		if (null != this.GetComponent<UGUIDrag> ()) {
			UGUIDrag target = this.GetComponent<UGUIDrag> ();
			target.enabled = isEnabled;
		}
		if (null != this.GetComponent<UIButton> ()) {
            UIButton target = this.GetComponent<UIButton> ();
			target.IsTouchEnabled = isEnabled;
		}
	}

	public void SetToolLock (bool isLock) {
		IsLockForTool = isLock;
		SetToolTouchEnabled (!(isLock || IsLock ()));
	}

	public void UnlockVideoLock (bool isUnlock) {
		if (IsLockForTool) {
			isUnlock = false;
		}
		SetToolTouchEnabled (isUnlock);
	}

}
