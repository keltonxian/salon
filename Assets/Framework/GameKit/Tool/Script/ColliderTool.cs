using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class ColliderTool : BaseTool
{
    // Event
    public bool _isSendEventOnce = false;
    private int _sendHoverEventCount = 0;
    private int _sendHoverOutEventCount = 0;
    private int _sendDropEventCount = 0;
    [SerializeField]
    protected Callback.UnityEventV _onHoverCollision = new Callback.UnityEventV();
    public Callback.UnityEventV OnHoverCollision
    {
        get
        {
            return _onHoverCollision;
        }
    }
    [SerializeField]
    protected Callback.UnityEventV _onHoverOutCollision = new Callback.UnityEventV();
    public Callback.UnityEventV OnHoverOutCollision
    {
        get
        {
            return _onHoverOutCollision;
        }
    }
    [SerializeField]
    protected Callback.UnityEventV _onDropCollision = new Callback.UnityEventV();
    public Callback.UnityEventV OnDropCollision
    {
        get
        {
            return _onDropCollision;
        }
    }
    // Trigger
    public bool _isTriggerAll = false;
    // Collider
    private Collider2D _currentCollider = null;
    public Collider2D CurrentCollider
    {
        get
        {
            return _currentCollider;
        }
    }
    public GameObject[] _arrayColliderObject;
    private List<Collider2D> _listCollider = new List<Collider2D>();
    public List<Collider2D> ListCollider
    {
        get
        {
            return _listCollider;
        }
    }
    // AudioClip
    public AudioClip _soundOnHover;
    private string _soundOnHoverID;

    private Coroutine _coOnHoverOut;

    public bool _isCheckTriggerState = false;
    public bool _defaultCheckTriggerState = false;
    private Dictionary<Collider2D, bool> _dicTriggerState = new Dictionary<Collider2D, bool>();

    public override void Init()
    {
        base.Init();
        SetupArgs();
    }

    public void Init(GameObject[] arrayColliderObject)
    {
        base.Init();
        SetupArgs(arrayColliderObject);
    }

    private void SetupArgs(GameObject[] arrayColliderObject = null)
    {
        _drag.IsSendHoverEvent = true;

        RefreshListCollider(arrayColliderObject);
    }

    public override void OnHover(UGUIDrag drag, Collider2D[] arrayCollider)
    {
        if (true == _isSendEventOnce && _sendHoverEventCount > 0)
        {
            return;
        }
        if (0 == _listCollider.Count)
        {
            return;
        }
        base.OnHover(drag, arrayCollider);
        bool flag = false;
        for (int i = 0; i < arrayCollider.Length; i++)
        {
            Collider2D c1 = arrayCollider[i];
            for (int j = 0; j < _listCollider.Count; j++)
            {
                Collider2D c2 = _listCollider[j];
                if (c1.transform == c2.transform )
                {
                    bool hasTrigger = false;
                    if (true == _isCheckTriggerState && true == _dicTriggerState[c2])
                    {
                        hasTrigger = true;
                    }
                    if (false == hasTrigger)
                    {
                        _currentCollider = c1;
                        OnHoverCollision.Invoke();
                        flag = true;
                        if (false == _isTriggerAll)
                        {
                            break;
                        }
                    }
                }
            }
            if (false == _isTriggerAll && true == flag)
            {
                break;
            }
        }
        if (true == flag)
        {
            _sendHoverEventCount += 1;
            if (_isControlDragEffect && _drag._dragEffectType == UGUIDrag.DRAG_EFFECT_TYPE.IN_AREA)
            {
                SetDragEffectPlay(true);
            }
            return;
        }
        SetOnHoverOutCoPlay(true);
    }

    public override void OnHoverOut(UGUIDrag drag)
    {
        if (true == _isSendEventOnce && _sendHoverOutEventCount > 0)
        {
            return;
        }
        if (0 == _listCollider.Count)
        {
            return;
        }
        base.OnHoverOut(drag);
        _sendHoverOutEventCount += 1;
        SetOnHoverOutCoPlay(true);
        OnHoverOutCollision.Invoke();
    }

    public override void OnDrop(UGUIDrag drag, Collider2D[] arrayCollider)
    {
        if (true == _isSendEventOnce && _sendDropEventCount > 0)
        {
            return;
        }
        if (0 == _listCollider.Count)
        {
            return;
        }
        base.OnDrop(drag, arrayCollider);
        _sendDropEventCount += 1;
        if (_isControlDragEffect && _drag._dragEffectType == UGUIDrag.DRAG_EFFECT_TYPE.IN_AREA)
        {
            SetDragEffectPlay(false);
        }
        OnDropCollision.Invoke();
    }

    private void SetOnHoverOutCoPlay(bool isPlay)
    {
        if (false == isPlay && null != _coOnHoverOut)
        {
            StopCoroutine(_coOnHoverOut);
            _coOnHoverOut = null;
        }
        else if (true == isPlay && null == _coOnHoverOut)
        {
            _coOnHoverOut = StartCoroutine(SetOnHoverOutFunc());
        }
    }

    private IEnumerator SetOnHoverOutFunc()
    {
        yield return new WaitForSeconds(0.5f);
        if (_isControlDragEffect && _drag._dragEffectType == UGUIDrag.DRAG_EFFECT_TYPE.IN_AREA)
        {
            SetDragEffectPlay(false);
        }
        _coOnHoverOut = null;
    }

    public void RefreshListCollider(GameObject[] arrayColliderObject = null)
    {
        _listCollider.Clear();
        GameObject[] arrayObj = _arrayColliderObject;
        if (null != arrayColliderObject)
        {
            arrayObj = _arrayColliderObject.Concat(arrayColliderObject).ToArray();
        }
        for (int i = 0; i < arrayObj.Length; i++)
        {
            GameObject temp = arrayObj[i];
            Collider2D[] array = temp.GetComponentsInChildren<Collider2D>();
            for (int j = 0; j < array.Length; j++)
            {
                _listCollider.Add(array[j]);
            }
        }
        for (int i = 0; i < _listCollider.Count; i++)
        {
            _dicTriggerState.Add(_listCollider[i], _defaultCheckTriggerState);
        }
    }

    public void SetColliderTriggerState(Collider2D collider, bool isTrigger)
    {
        _dicTriggerState[collider] = isTrigger;
    }

    public bool GetColliderTriggerState(Collider2D collider)
    {
        return _dicTriggerState[collider];
    }

    public void SetListColliderEnabled(bool isEnabled)
    {
        foreach(Collider2D c in _listCollider)
        {
            c.enabled = isEnabled;
        }
    }

    public void RemoveCollider(Collider2D c)
    {
        _listCollider.Remove(c);
    }

    public override void OnDragToolBegin(UGUIDrag drag, PointerEventData eventData)
    {
        base.OnDragToolBegin(drag, eventData);
        SetListColliderEnabled(true);
        _sendHoverEventCount = 0;
        _sendHoverOutEventCount = 0;
        _sendDropEventCount = 0;
    }

    public override void OnDragToolEnd(UGUIDrag drag, PointerEventData eventData)
    {
        base.OnDragToolEnd(drag, eventData);
        SetListColliderEnabled(false);
    }

    public override void OnDragToolTweenOver(UGUIDrag drag)
    {
        base.OnDragToolTweenOver(drag);
    }

    public override void ResetStatus()
    {
        if (false == _isKeepStatus)
        {
            SetListColliderEnabled(false);
        }
    }

    public void ResetTarget()
    {
        for (int i = 0; i < _listCollider.Count; i++)
        {
            Collider2D c = _listCollider[i];
            c.enabled = true;
            c.gameObject.SetActive(true);
        }
    }

}
