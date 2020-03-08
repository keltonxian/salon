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

    private void OnEnable()
    {
        PluginManager.OnImageSaveSuccessed += OnImageSaveSuccessed;
        PluginManager.OnImageSaveFailed += OnImageSaveFailed;
    }

    private void OnDisable()
    {
        PluginManager.OnImageSaveSuccessed -= OnImageSaveSuccessed;
        PluginManager.OnImageSaveFailed -= OnImageSaveFailed;
    }

    private void OnImageSaveSuccessed()
    {
        GameManager.Log("OnImageSaveSuccessed");
        OnClickClose();
    }

    private void OnImageSaveFailed()
    {
        GameManager.Log("OnImageSaveFailed");
    }

    public void OnClickSave()
    {
        PluginManager.OnPermissionAction = (bool isHasPermission, string code) =>
        {
            GameManager.Log(string.Format("isHasPermission[{0}] code[{1}]", isHasPermission.ToString(), code));
            if (true == isHasPermission)
            {
                byte[] bytes = _pic.sprite.texture.EncodeToPNG();
                if (0 == bytes.Length)
                {
                    return;
                }
                PluginManager.sharePlugin.SaveImage("Screenshot", bytes, bytes.Length);
            }
            else
            {
                //if Util.IsAndroid() then
                //    self:OnImageSaveFailed()
                //end
            }
        };
        PluginManager.RequestPermission();
    }
}
