using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using PureMVC.Core;

public class PopupView : Base
{
    public enum AnimType
    {
        TopToCenter, ScaleFromCenter, ScaleFromPos,
    }

    public AnimType _animType = AnimType.TopToCenter;
    public Canvas _canvas;
    public Transform _background;
    public Transform _blockView;
    public RectTransform _popup;
    private Vector2 _markPopupPos;
    private Vector3 _markPopupScale;
    public Transform _tipParent;
    private GameObject _tipFinger;
    public Transform _scaleFromTarget;

    private bool _isDone = false;

    [SerializeField]
    private Callback.UnityEventI _onSuccess = new Callback.UnityEventI();
    public Callback.UnityEventI OnSuccess
    {
        get
        {
            return _onSuccess;
        }
    }

    [SerializeField]
    private Callback.UnityEventV _onFail = new Callback.UnityEventV();
    public Callback.UnityEventV OnFail
    {
        get
        {
            return _onFail;
        }
    }

    void OnDestroy()
    {
        HideTipFinger();
    }

    void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        _markPopupPos = _popup.anchoredPosition;
        _markPopupScale = _popup.localScale;
        _popup.anchoredPosition = new Vector2(1024, 1024);
        _canvas.worldCamera = Camera.main;
    }

    public virtual void StartShow()
    {
        Show(() =>
        {
            Play();
        });
    }

    protected void SetBackgroundShow(bool isShow)
    {
        if (null == _background)
        {
            return;
        }
        _background.gameObject.SetActive(isShow);
    }

    private void SetBlockViewShow(bool isShow)
    {
        if (null == _blockView)
        {
            return;
        }
        _blockView.gameObject.SetActive(isShow);
    }

    protected void ShowBlockView()
    {
        SetBlockViewShow(true);
    }

    protected void HideBlockView()
    {
        SetBlockViewShow(false);
    }

    protected void ShowByTopToCenter(Action callback)
    {
        Vector2 pos = _markPopupPos;
        _popup.anchoredPosition = new Vector2(_markPopupPos.x, 1024);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            UIManager.PlaySoundPopupShow();
        });
        sequence.Append(_popup.DOAnchorPos(pos, 0.6f).SetEase(Ease.OutBack));
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }

    protected void HideByTopToCenter(Action callback)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.1f);
        sequence.AppendCallback(() =>
        {
            UIManager.PlaySoundPopupHide();
        });
        sequence.Append(_popup.DOAnchorPosY(1024, 0.6f).SetEase(Ease.InBack));
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }

    protected void ShowByScaleFromCenter(Action callback)
    {
        _popup.anchoredPosition = _markPopupPos;
        Vector3 scale = _markPopupScale;
        _popup.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            UIManager.PlaySoundPopupShow();
        });
        sequence.Append(_popup.DOScale(scale, 0.6f).SetEase(Ease.OutBack));
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }

    protected void HideByScaleFromCenter(Action callback)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.1f);
        sequence.AppendCallback(() =>
        {
            UIManager.PlaySoundPopupHide();
        });
        sequence.Append(_popup.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InBack));
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }

    protected void ShowByScaleFromPos(Action callback)
    {
        _popup.anchoredPosition = _markPopupPos;
        Vector3 toPos = _popup.position;
        _popup.position = _scaleFromTarget.position;
        Vector3 scale = _markPopupScale;
        _popup.localScale = Vector3.zero;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() =>
        {
            UIManager.PlaySoundPopupShow();
        });
        sequence.Append(_popup.DOMove(toPos, 0.6f).SetEase(Ease.Linear));
        sequence.Join(_popup.DOScale(scale, 0.6f).SetEase(Ease.OutBack));
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }

    protected void HideByScaleFromPos(Action callback)
    {
        Vector3 toPos = _scaleFromTarget.position;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.1f);
        sequence.AppendCallback(() =>
        {
            UIManager.PlaySoundPopupHide();
        });
        sequence.Append(_popup.DOMove(toPos, 0.6f).SetEase(Ease.Linear));
        sequence.Append(_popup.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InBack));
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }

    protected void Show(Action callback)
    {
        SetBackgroundShow(true);
        if (_animType == AnimType.TopToCenter)
        {
            ShowByTopToCenter(callback);
        }
        else if (_animType == AnimType.ScaleFromCenter)
        {
            ShowByScaleFromCenter(callback);
        }
        else if (_animType == AnimType.ScaleFromPos)
        {
            ShowByScaleFromPos(callback);
        }
    }

    protected void Hide(Action callback)
    {
        SetBackgroundShow(false);
        if (_animType == AnimType.TopToCenter)
        {
            HideByTopToCenter(callback);
        }
        else if (_animType == AnimType.ScaleFromCenter)
        {
            HideByScaleFromCenter(callback);
        }
        else if (_animType == AnimType.ScaleFromPos)
        {
            HideByScaleFromPos(callback);
        }
    }

    protected void ShowTipFinger(Transform fromObj, Transform toObj, float speed, LoopType loopType)
    {
        UIManager.ShowTipFinger(_tipParent, (GameObject tip) =>
        {
            _tipFinger = tip;
        }, fromObj, toObj, speed, loopType);
    }

    protected void HideTipFinger()
    {
        UIManager.HideTipFinger(_tipFinger);
    }

    protected virtual void Play()
    {

    }

    protected void Close()
    {
        if (true == _isDone)
        {
            return;
        }
        _isDone = true;
        DoClose(false);
    }

    protected void DoClose(bool isDone, int index = 0)
    {
        Hide(() =>
        {
            if (true == isDone)
            {
                OnSuccess.Invoke(index);
            }
            else
            {
                OnFail.Invoke();
            }
            DestroyAll();
        });
    }

    protected virtual void DestroyAll()
    {
        Destroy(this.gameObject);
    }

}
