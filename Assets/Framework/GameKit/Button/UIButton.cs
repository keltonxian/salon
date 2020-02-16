using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : BaseButton, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public enum EventType
    {
        TOUCH, CLICK
    }
    public EventType _eventType = EventType.CLICK;
    private bool _isTouchDown = false;

    [SerializeField]
    protected Callback.UnityEventPE _onTouchDown = new Callback.UnityEventPE();
    public Callback.UnityEventPE OnTouchDown
    {
        get
        {
            return _onTouchDown;
        }
    }
    [SerializeField]
    protected Callback.UnityEventPE _onTouchUp = new Callback.UnityEventPE();
    public Callback.UnityEventPE OnTouchUp
    {
        get
        {
            return _onTouchUp;
        }
    }
    [SerializeField]
    protected Callback.UnityEventPE _onClick = new Callback.UnityEventPE();
    public Callback.UnityEventPE OnClick
    {
        get
        {
            return _onClick;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_eventType != EventType.TOUCH)
        {
            return;
        }
        if (true != CheckCanClick())
        {
            return;
        }
        _timeWaitNextClick = _clickInterval;
        _isTouchDown = true;
        PlayTouchSound();
        PlayTouchAnim(() =>
        {
            OnTouchDown.Invoke(eventData);
        });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_eventType != EventType.TOUCH)
        {
            return;
        }
        if (true != _isTouchDown)
        {
            return;
        }
        _isTouchDown = false;
        OnTouchUp.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_eventType != EventType.CLICK)
        {
            return;
        }
        if (true != CheckCanClick())
        {
            return;
        }
        _timeWaitNextClick = _clickInterval;
        PlayTouchSound();
        PlayTouchAnim(() =>
        {
            OnClick.Invoke(eventData);
        });
    }
}
