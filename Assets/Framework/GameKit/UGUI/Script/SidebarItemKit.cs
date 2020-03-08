using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Core;

[RequireComponent(typeof(LockItemKit))]
public class SidebarItemKit : Base {

	public string _iconFormat = null;
	public int _itemCount = 0;
	public Vector3 _itemScale = Vector3.one;
	public Vector3 _itemPosOffset = Vector3.zero;
	public string _pathBtnReset = "SidebarItemReset";

	public bool _isHorizontal = false;

	[Header("Click Item Callback")]
	[SerializeField]
	private Callback.UnityEventI _onClick = new Callback.UnityEventI ();
	public Callback.UnityEventI OnClickEvent {
		get {
			return _onClick;
		}
	}

	public GameObject _itemPrefab;

	private float _containerDefaultX = 300f;
	private float _containerGapX = 200f;

	private Sequence _animKTButtonSeq = null;

	private LockItemKit _lockItemKit;

	public void ItemClick (int index) {
		_onClick.Invoke (index);
		// get the method assigned in the editor and call it
//		MethodInfo methodInfo = UnityEventBase.GetValidMethodInfo(_onClick.GetPersistentTarget(0), _onClick.GetPersistentMethodName(0), new Type[] { typeof(int) } );
//		methodInfo.Invoke(GameObject.FindObjectOfType<MakeupViewController>(), new object[] { index });
	}

	void Start () {
		// this is to turn off the value and call set in the editor
//		_onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
//		_onClick.RemoveAllListeners();

		_lockItemKit = this.GetComponent<LockItemKit> ();
	}
	
	void Update () {
	}

	private void AddLock (RectTransform item, int index) {
		if (0 == index) {
			return;
		}
		bool isLock = true;
		if (index % 2 != 0) {
			isLock = false;
		}
		if (false == isLock) {
			return;
		}
		LockItemKit lockItemKit = item.gameObject.AddComponent<LockItemKit> ();
		lockItemKit._isAutoAddLock = false;
		lockItemKit._iapType = _lockItemKit._iapType;
		lockItemKit._lockKey =  string.Format ("{0}_{1}", _lockItemKit._lockKey, index);
		lockItemKit._lockSprite = _lockItemKit._lockSprite;
		lockItemKit._lockOffset = _lockItemKit._lockOffset;
		lockItemKit._lockScale = _lockItemKit._lockScale;
		lockItemKit._lockAreaSize = _lockItemKit._lockAreaSize;
		lockItemKit.AddLock ((bool isUnlock) => {
			if (isUnlock) {
				UnlockAll ();
			}
		}, (bool isUnlock) => {
			lockItemKit.UnlockVideoLock (isUnlock);
		});
		Transform itemLock = lockItemKit.transform.Find ("ItemLock");
		if (null != itemLock) {
			itemLock.GetComponent<Image> ().raycastTarget = false;
			itemLock.Find ("ItemLockImage").GetComponent<Image> ().raycastTarget = false;
			itemLock.GetComponent<UIButton> ().IsTouchEnabled = false;
		}
	}

	public void UnlockAll () {
		float itemOffset = 0f;
		float gap = 0f;
		if (_isHorizontal) {
			HorizontalLayoutGroup layout = this.GetComponent<HorizontalLayoutGroup> ();
			gap = layout.spacing;
		} else {
			VerticalLayoutGroup layout = this.GetComponent<VerticalLayoutGroup> ();
			gap = layout.spacing;
		}
		bool hasPlaySound = false;
		RectTransform contentRT = this.GetComponent<RectTransform> ().parent.GetComponent<RectTransform> ();
		RectTransform scrollRT = this.GetComponent<RectTransform> ().parent.parent.parent.GetComponent<RectTransform> ();
		RectTransform parent = this.GetComponent<RectTransform> ();
		for (int i = 0; i < parent.childCount; i++) {
			RectTransform itemRT = parent.GetChild (i).GetComponent<RectTransform> ();
			bool isPlaySound = false;
			bool isPlayParticle = false;
			if (_isHorizontal) {
				itemOffset += itemRT.sizeDelta.x;
				if (-contentRT.anchoredPosition.x < itemOffset && -contentRT.anchoredPosition.x + scrollRT.sizeDelta.x > itemOffset) {
					isPlayParticle = true;
				}
			} else {
				itemOffset += itemRT.sizeDelta.y;
				if (contentRT.anchoredPosition.y < itemOffset && contentRT.anchoredPosition.y + scrollRT.sizeDelta.y > itemOffset) {
					isPlayParticle = true;
				}
			}
			itemOffset += gap;
			LockItemKit lockItemKit = itemRT.GetComponent<LockItemKit> ();
			if (null == lockItemKit || !lockItemKit.IsLock ()) {
				continue;
			}
			if (false == hasPlaySound) {
				hasPlaySound = true;
				isPlaySound = true;
			}
			lockItemKit.RemoveLock (isPlayParticle, isPlaySound);
		}
	}

	private void Unlock (RectTransform item, int index) {
		LockItemKit lockitemKit = item.GetComponent<LockItemKit> ();
		if (null == lockitemKit || !lockitemKit.IsLock ()) {
			return;
		}
		lockitemKit.Unlock ();
	}

	public void InitItem (Callback.CallbackV callback = null) {
		RectTransform container = gameObject.GetComponent<RectTransform> ();
		container.localPosition += new Vector3 (_containerDefaultX + _containerGapX * 0, 0f, 0f);
		float gapX = 0f;
		float gapY = 0f;
		bool isControlSize = false;
		if (_isHorizontal) {
			HorizontalLayoutGroup layout = container.GetComponent<HorizontalLayoutGroup> ();
			gapX = layout.spacing;
			if (true == layout.childControlWidth && true == layout.childControlHeight) {
				isControlSize = true;
			}
		} else {
			VerticalLayoutGroup layout = container.GetComponent<VerticalLayoutGroup> ();
			gapY = layout.spacing;
			if (true == layout.childControlWidth && true == layout.childControlHeight) {
				isControlSize = true;
			}
		}
		bool isNeedReset = !string.IsNullOrEmpty (_pathBtnReset);
		GameObject prefab = _itemPrefab;
		string iconFormat = _iconFormat;
		Vector3 itemScale = _itemScale;
		Vector3 itemPosOffset = _itemPosOffset;
		int count = _itemCount;
		if (true == isNeedReset) {
			count++;
		}
		int countAdd = 0;
		Vector2 containerSize = Vector2.zero;
		for (int j = 0; j < count; j++) {
			GameObject item = Instantiate (prefab) as GameObject;
			int index = j;
			if (false == isNeedReset) {
				index++;
			}
			RectTransform itemRT = item.GetComponent<RectTransform> ();
			UIButton ktButton = item.AddComponent<UIButton> ();
            ktButton._effectType = UIButton.EffectType.NONE;
            ktButton._eventType = UIButton.EventType.CLICK;
			//ktButton._clickSoundPath = GameResPath.SFX_ITEM_SELECT;
			item.GetComponent<Toggle> ().onValueChanged.AddListener ((bool isOn) => {
				if (!isOn) {
					if (null != _animKTButtonSeq) {
						_animKTButtonSeq.Kill ();
						_animKTButtonSeq = null;
					}
					ktButton.StopTouchAnim ();
					itemRT.localScale = itemScale;
//					Debug.Log("itemRT.localScale:"+itemRT.localScale);
					return;
				}
				Sequence seq = DOTween.Sequence ();
				_animKTButtonSeq = seq;
				float time = 0.25f;
				float scaleChange = 0.2f;
				seq.Append (itemRT.DOScale (new Vector3 (itemScale.x + scaleChange, itemScale.y - scaleChange, 1f), time));
				seq.Append (itemRT.DOScale (new Vector3 (itemScale.x - scaleChange, itemScale.y + scaleChange, 1f), time));
				seq.Append (itemRT.DOScale (new Vector3 (itemScale.x + scaleChange, itemScale.y + scaleChange, 1f), time));
				seq.AppendCallback (() => {
					LockItemKit lockKit = itemRT.GetComponent<LockItemKit> ();
					if (null != lockKit && lockKit.IsLock ()) {
						lockKit.Unlock ();
						return;
					}
					OnClickEvent.Invoke (index);
				});
//				AudioManager.Instance.PlaySoundEffect (GameResPath.SFX_ITEM_SELECT);
//				ktButton.PlayTouchAnim (()=>{
//				});
			});
			itemRT.SetParent (container);
			item.GetComponent<Toggle> ().isOn = false;
			item.GetComponent<Toggle> ().group = container.GetComponent<ToggleGroup> ();
//			Vector2 bigScale = itemScale + new Vector3 (0.2f, 0.2f, 0f);
//			ktButton.SetScaleDefault (bigScale.x, bigScale.y);
			AddLock (itemRT, index);
			Vector3 pos = itemRT.localPosition;
			itemRT.localPosition = new Vector3 (pos.x, pos.y, 0);
			itemRT.localScale = itemScale;
			RectTransform iconRT = itemRT.GetChild (0) as RectTransform;
			RectTransform checkerRT = itemRT.GetChild (1) as RectTransform;
			Image iconImage = iconRT.GetComponent<Image> ();
			string url = null;
			if (true == isNeedReset && 0 == j) {
				url = _pathBtnReset;
			} else {
				url = string.Format (iconFormat, index);
			}
			countAdd++;
			ResourcesManager.LoadSpriteFromPrefab (url, iconImage, (bool isDone) => {
				Vector2 size = iconImage.rectTransform.sizeDelta;
				size.x *= itemScale.x;
				size.y *= itemScale.y;
				checkerRT.sizeDelta = size;
				if (false == isControlSize) {
					itemRT.sizeDelta = size;
				}
				if (_isHorizontal) {
					containerSize.x += itemRT.sizeDelta.x + gapX;
					containerSize.y = itemRT.sizeDelta.y;
				} else {
					containerSize.x = itemRT.sizeDelta.x;
					containerSize.y += itemRT.sizeDelta.y + gapY;
				}
				iconRT.localPosition += itemPosOffset;
				checkerRT.localPosition += itemPosOffset;
			});
			if (countAdd == count) {
				container.sizeDelta = containerSize;
				if (null != callback) {
					callback ();
				}
			}
		}
	}

}
