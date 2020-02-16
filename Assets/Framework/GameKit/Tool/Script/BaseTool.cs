using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using PureMVC.Core;

public class BaseTool : Base
{
    // Component
    public UGUIDrag _drag;
    protected bool _isSelected = false;
    protected bool _isKeepStatus = false;
    public bool _isControlDragEffect = false;
    // Tool Event
    [SerializeField]
    private Callback.UnityEventV _onToolPreStart = new Callback.UnityEventV();
    public Callback.UnityEventV OnToolPreStart
    {
        get
        {
            return _onToolPreStart;
        }
    }
    [SerializeField]
    private Callback.UnityEventV _onToolStart = new Callback.UnityEventV();
    public Callback.UnityEventV OnToolStart
    {
        get
        {
            return _onToolStart;
        }
    }
    [SerializeField]
    private Callback.UnityEventV _onToolEnd = new Callback.UnityEventV();
    public Callback.UnityEventV OnToolEnd
    {
        get
        {
            return _onToolEnd;
        }
    }
    [SerializeField]
    private Callback.UnityEventV _onToolClick = new Callback.UnityEventV();
    public Callback.UnityEventV OnToolClick
    {
        get
        {
            return _onToolClick;
        }
    }
    [SerializeField]
    private Callback.UnityEventV _onToolTweenOver = new Callback.UnityEventV();
    public Callback.UnityEventV OnToolTweenOver
    {
        get
        {
            return _onToolTweenOver;
        }
    }

    public virtual void Init()
    {
        if (null == _drag)
        {
            _drag = GetComponent<UGUIDrag>();
        }
        if (null != _drag)
        {
            _drag.OnBeginDragAction += OnDragToolBegin;
            _drag.OnDragAction += OnDragTool;
            _drag.OnEndDragAction += OnDragToolEnd;
            _drag.OnTweenOverAction += OnDragToolTweenOver;
            _drag.OnHoverColliderAction += OnHover;
            _drag.OnHoverColliderOutAction += OnHoverOut;
            _drag.OnDropColliderAction += OnDrop;
            _drag.OnClickAction += OnClick;
        }
    }

    public virtual void InitStatus()
    {
        _drag.InitStatus();
    }

    public virtual void ResetStatus()
    {
        _drag.ResetStatus();
    }

    public virtual void UnSelected()
    {
        _drag.UnSelected();
    }

    public virtual void ResetAnim()
    {
        _drag.ResetAnim();
    }

    public virtual void SetDragEffectPlay(bool isPlay)
    {
        _drag.SetDragEffectPlay(isPlay);
    }

    protected void BackPosition(UGUIDrag.DragBackEffect backEffect = UGUIDrag.DragBackEffect.None)
    {
        _drag.BackPosition(backEffect);
    }

    public virtual void OnDragToolBegin(UGUIDrag drag, PointerEventData eventData)
    {
        OnToolPreStart.Invoke();

        OnToolStart.Invoke();
    }

    public virtual void OnDragTool(UGUIDrag drag, PointerEventData eventData)
    {
        
    }

    public virtual void OnDragToolEnd(UGUIDrag drag, PointerEventData eventData)
    {
        OnToolEnd.Invoke();
    }

    public virtual void OnDragToolTweenOver(UGUIDrag drag)
    {
        OnToolTweenOver.Invoke();
    }

    public virtual void OnHover(UGUIDrag drag, Collider2D[] arrayCollider)
    {

    }

    public virtual void OnHoverOut(UGUIDrag drag)
    {

    }

    public virtual void OnDrop(UGUIDrag drag, Collider2D[] arrayCollider)
    {

    }

    public virtual void OnClick(UGUIDrag drag)
    {

    }

    public bool IsRaycastLocationValid()
    {
        return (!_drag.IsDragging);
    }

    public void CancelDrag()
    {
        if (null != _drag)
        {
            if (true == _drag.IsDragging)
            {
                _drag.SetEnabled(false);
                BackPosition();
            }
        }
    }
}
