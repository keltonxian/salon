using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PureMVC.Manager;
using PureMVC.Const;
using UnityEngine.UI;
using DG.Tweening;

public class Screenshot : PopupView
{
    private static string _bundleUrlUI = string.Format("{0}/{1}_screenshot.unity3d", AppConst.AppName, AppConst.AppName);

    public Image _pic;
    public AudioClip _soundCapture;
    public Image _flash;
    private GameObject _targetView;
    public GameObject TargetView
    {
        set
        {
            _targetView = value;
            StartShow();
        }
    }

    public static void Create(Action<Screenshot> callback)
    {
        GameManager.Instance.AssetBundleManager.LoadAsset(_bundleUrlUI, (AssetBundleManager.Asset asset) =>
        {
            GameObject prefab = asset._assetBundle.LoadAsset<GameObject>("Screenshot");
            if (null == prefab)
            {
                return;
            }
            Screenshot popup = Instantiate(prefab).GetComponent<Screenshot>();
            popup.gameObject.name = prefab.name;
            callback.Invoke(popup);
        });
    }

    protected override void DestroyAll()
    {
        base.DestroyAll();
        AssetBundleManager.DisposeAsset(_bundleUrlUI);
    }

    public override void StartShow()
    {
        GameObject captureCanvas = GameObject.Find("CaptureCanvas");
        if (null != captureCanvas)
        {
            CaptureKit captureKit = captureCanvas.GetComponent<CaptureKit>();
            captureKit.Init();
            Sprite sprite = captureKit.CaptureSprite(_targetView);
            _pic.sprite = sprite;
            _pic.SetNativeSize();
            AudioManager.PlaySound(_soundCapture);
            _flash.enabled = true;
            _flash.DOFade(0, 0.5f);
        }
        base.StartShow();
    }

    public void OnClickClose()
    {
        Close();
    }

    public void OnClickSave()
    {

    }
}
