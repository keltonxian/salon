using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Manager;

public class ShopController : ShopHandler
{
	public RectTransform _fullPack;

	private LockManager.IAP_TYPE _miniShopType = LockManager.IAP_TYPE.NONE;
	public LockManager.IAP_TYPE MiniShopType {
		get {
			return _miniShopType;
		}
		set {
			_miniShopType = value;
			//ShowMiniShopPack ();
		}
	}

    private void Start()
    {
        StartShow();
    }

    public override void Init () {
        base.Init();
        if (null != _fullPack)
        {
            _fullPack.transform.DOScale(0.06f, 0.8f).SetRelative().SetLoops(-1, LoopType.Yoyo);
        }
    }

	private void LoopEnableImage (RectTransform item, bool isEnabled) {
        if (null == item)
        {
            return;
        }
		Image image = item.GetComponent<Image> ();
		if (null != image && null != image.sprite) {
			float color = isEnabled ? 1f : 0.5f;
			image.raycastTarget = isEnabled;
			image.color = new Color (color, color, color, 1f);
		}
		if (0 == item.childCount) {
			return;
		}
		for (int i = 0; i < item.childCount; i++) {
			RectTransform subItem = item.GetChild (i).GetComponent<RectTransform> ();
			LoopEnableImage (subItem, isEnabled);
		}
	}

	public void OnClickFullPack () {
		UnlockFull ();
	}

	public void OnClickRestore () {
		Restore ();
	}

	public void OnClickGetItForFree () {
		GetItForFree ();
	}

}
