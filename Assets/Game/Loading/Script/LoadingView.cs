using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Patterns.Facade;
using PureMVC.Const;
using System;
using PureMVC.Manager;
using PureMVC.Core;

public class LoadingView : Base
{
    public Image _progress;
    public Transform _mask;

    void Awake()
    {

    }

    public void Enter()
    {
        InitModel(() =>
        {
            _progress.DOFillAmount(1f, 3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Facade.Instance.SendMessageCommand(NotiConst.LOADING_END);
            });
        });
    }

    private void InitModel(Action callback)
    {
        string url = string.Format("{0}/{1}_loading_{2}.unity3d", AppConst.AppName, AppConst.AppName, GameManager.Role.ToString()).ToLower();
        AssetBundleManager.LoadAsset(url, (AssetBundleManager.Asset asset) => {
            GameObject prefab = asset._assetBundle.LoadAsset<GameObject>(GameManager.Role.ToString());
            Transform model = Instantiate(prefab).transform;
            model.gameObject.name = prefab.name;
            Vector3 pos = model.localPosition;
            Vector3 scale = model.localScale;
            model.SetParent(_mask);
            model.localPosition = pos;
            model.localScale = scale;
            BeardModelHandler handler = model.GetComponent<BeardModelHandler>();
            handler.Init();
            handler.StartEmotion();
            callback.Invoke();
        }, (AssetBundleManager.Asset asset) => {
        });
    }
}
