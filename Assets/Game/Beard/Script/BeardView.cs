using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Patterns.Facade;
using PureMVC.Const;
using PureMVC.Core;
using PureMVC.Manager;
using System;

public class BeardView : Base
{
    public GameObject _view;
    public RectTransform _seat;
    private Vector2 _markSeatPos;
    public Transform _modelParent;
    public BeardModelHandler _model;
    public PageView _pageView;
    public BeardToolScissors _scissors;
    public BeardToolGrower _grower;
    private Vector2 _markGrowerPos;
    public BeardToolBubble _bubble;
    private Vector2 _markBubblePos;
    public BeardToolShaver _shaverRound;
    public BeardToolShaver _shaverSharp;
    public BeardToolScraper _scraper;
    public BeardToolRazor _razor;
    public BeardToolForceps _forceps;
    public BeardToolBandage _bandage;
    public BeardToolUnguent _unguent;
    public BeardToolCream _cream;
    public BeardFaceRed _faceRed;
    public BeardFaceScar _faceScar;
    public BeardToolDye _dyeRed;
    public BeardToolDye _dyeGreen;
    public BeardToolDye _dyeBlue;
    public BeardToolDye _dyePurple;

    private bool _isInBeard = true;
    private bool _isSeatDefault = true;

    void Awake()
    {

    }

    public void Enter()
    {
        InitModel(()=> {
            SetupTool();
        });
    }

    private void SetupTool()
    {
        _markSeatPos = _seat.anchoredPosition;
        _model.Init();
        _pageView.Init();
        _scissors.Init(_model);
        _grower.Init(_model);
        _markGrowerPos = _grower.GetComponent<RectTransform>().anchoredPosition;
        _bubble.Init(_model);
        _markBubblePos = _bubble.GetComponent<RectTransform>().anchoredPosition;
        _shaverRound.Init(_model);
        _shaverSharp.Init(_model);
        _scraper.Init(_model);
        _razor.Init(_model);
        _forceps.Init(_model);
        _bandage.Init(_model);
        _unguent.Init(_model);
        _cream.Init(_model);
        _dyeRed.Init(_model);
        _dyeGreen.Init(_model);
        _dyeBlue.Init(_model);
        _dyePurple.Init(_model);
        SetCheckToolDrag(true);
        //Facade.Instance.SendMessageCommand(NotiConst.BEARD_END, "");

        _faceRed.Init(_model);
        _faceScar.Init(_model);

        _model.StartEmotion();

        //int debugIndex = 3;
        //StartCoroutine(GoDebug(debugIndex));
    }

    private void SetCheckToolDrag(bool isSet)
    {
        _pageView.SetCheckToolDrag(_scissors.ToolCollider, isSet);
        _pageView.SetCheckToolDrag(_grower.ToolCollider, isSet);
        _pageView.SetCheckToolDrag(_bubble.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_shaverRound.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_shaverSharp.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_scraper.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_razor.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_forceps.ToolCollider, isSet);
        _pageView.SetCheckToolDrag(_bandage.ToolCollider, isSet);
        _pageView.SetCheckToolDrag(_unguent.ToolCollider, isSet);
        _pageView.SetCheckToolDrag(_cream.ToolCollider, isSet);
        _pageView.SetCheckToolDrag(_dyeRed.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_dyeGreen.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_dyeBlue.ToolScribble, isSet);
        _pageView.SetCheckToolDrag(_dyePurple.ToolScribble, isSet);
    }

    private IEnumerator GoDebug(int index)
    {
        switch (index)
        {
            case 1:
                yield return new WaitForSeconds(1);
                _pageView.GotoPage(1);
                _scissors.ShowDone();
                break;
            case 2:
                yield return new WaitForSeconds(1);
                _pageView.GotoPage(2);
                _scissors.ShowDone();
                _shaverSharp.ShowDone();
                break;
            case 3:
                yield return new WaitForSeconds(1);
                _pageView.GotoPage(6);
                break;
        }
    }

    public void Exit()
    {
        SetCheckToolDrag(false);
    }

    private void InitModel(Action callback)
    {
        if (null != _model)
        {
            return;
        }
        //string path = string.Format("BeardModel/{0}", GameManager.Role.ToString());
        //GameObject prefab = Resources.Load<GameObject>(path);
        //Transform model = Instantiate(prefab).transform;
        //model.gameObject.name = prefab.name;
        //Vector3 pos = model.localPosition;
        //Vector3 scale = model.localScale;
        //model.SetParent(_modelParent);
        //model.localPosition = pos;
        //model.localScale = scale;
        //_model = model.GetComponent<BeardModelHandler>();
        string url = string.Format("{0}/{1}_{2}.unity3d", AppConst.AppName, AppConst.AppName, GameManager.Role.ToString()).ToLower();
        AssetBundleManager.LoadAsset(url, (AssetBundleManager.Asset asset) => {
            GameObject prefab = asset._assetBundle.LoadAsset<GameObject>(GameManager.Role.ToString());
            Transform model = Instantiate(prefab).transform;
            model.gameObject.name = prefab.name;
            Vector3 pos = model.localPosition;
            Vector3 scale = model.localScale;
            model.SetParent(_modelParent);
            model.localPosition = pos;
            model.localScale = scale;
            _model = model.GetComponent<BeardModelHandler>();
            callback.Invoke();
        }, (AssetBundleManager.Asset asset) => {
        });
    }

    public void OnClickPagePrev()
    {
        _pageView.PrevPage();
    }

    public void OnClickPageNext()
    {
        _pageView.NextPage();
    }

    public void OnPageChange(int currentPage)
    {
        Debug.Log("Change Page:" + currentPage);
        Sequence sequence = DOTween.Sequence();
        if (true == _isInBeard && currentPage >= 6)
        {
            _isInBeard = false;
            sequence.AppendCallback(() =>
            {
                _grower.GetComponent<RectTransform>().anchoredPosition = new Vector2(1024, 1024);
                _bubble.GetComponent<RectTransform>().anchoredPosition = new Vector2(1024, 1024);
            });
        }
        if (true == _isSeatDefault && 6 == currentPage)
        {
            _isSeatDefault = false;
            sequence.Append(_seat.DOAnchorPos(_markSeatPos + new Vector2(0f, -80f), 0.3f).SetEase(Ease.Linear));
        }
        else if (false == _isSeatDefault && 6 != currentPage)
        {
            _isSeatDefault = true;
            sequence.Append(_seat.DOAnchorPos(_markSeatPos, 0.3f).SetEase(Ease.Linear));
            
        }
        if (false == _isInBeard && currentPage < 6)
        {
            _isInBeard = true;
            sequence.AppendCallback(() =>
            {
                _grower.GetComponent<RectTransform>().anchoredPosition = _markGrowerPos;
                _bubble.GetComponent<RectTransform>().anchoredPosition = _markBubblePos;
            });
        }
    }

    public void OnScreenshot()
    {
        Screenshot.Create((Screenshot popup) => {
            popup.TargetView = _view;
        });
    }

}
