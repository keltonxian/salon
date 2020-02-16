using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

class PageItem : MonoBehaviour
{
    protected internal int index = 0;
}

[RequireComponent(typeof(RectTransform))]
public class PageView : ScrollRect, IPointerUpHandler, IPointerExitHandler, IPage
{
    private float _dragTime = 0f;
    private bool _isDrag = false;
    private Vector2 _endPos = Vector2.zero;

    private bool _isScrolling = false;
    private bool _hasStartDrag = false;
    private Vector3 _markBeginDragPos;
    private Vector3 _markDragPos;

    private int _totalPage = 0;
    public int TotalPage
    {
        get
        {
            return _totalPage;
        }
    }

    private int _currentPage = 0;
    public int CurrentPage
    {
        get
        {
            return content.GetChild(_currentPage).GetComponent<PageItem>().index;
        }
    }

    private bool _isUpdateEnabled = false;

    private Vector2 _contentPos = Vector2.zero;
    private PointerEventData _dragEventData;

    public PageIndicator _pageIndicator;
    public float _pageDamp = 0.2f;
    public bool _isDragOutSideEnabled = true;
    public bool _isAutoInit = true;
    public bool _isLoop = false;

    public Callback.UnityEventV _onScrollOver;
    public Callback.UnityEventI _onPageChange;

    public Action<Vector2> _onBeginDragAction;
    public Action<Vector2> _onDragAction;
    public Action<Vector2> _onEndDragAction;

    private bool _isIn = false;

    protected override void LateUpdate()
    {
        if (_isDrag)
        {
            base.LateUpdate();
        }
        else if (_isUpdateEnabled)
        {
            if (horizontal)
            {
                if (Mathf.Abs(content.anchoredPosition.x - _endPos.x) > 1f)
                {
                    Vector2 pos = Vector2.Lerp(content.anchoredPosition, _endPos, _pageDamp);
                    SetContentAnchoredPosition(pos);
                }
                else
                {
                    SetContentAnchoredPosition(_endPos);
                    _isUpdateEnabled = false;
                    _onScrollOver.Invoke();
                }
            }
            else if (vertical)
            {
                if (Mathf.Abs(content.anchoredPosition.y - _endPos.y) > 1f)
                {
                    Vector2 pos = Vector2.Lerp(content.anchoredPosition, _endPos, _pageDamp);
                    SetContentAnchoredPosition(pos);
                }
                else
                {
                    SetContentAnchoredPosition(_endPos);
                    _isUpdateEnabled = false;
                    _onScrollOver.Invoke();
                }
            }
        }
    }

    void OnApplicationFocus(bool flag)
    {
        if (!flag)
        {
            _isIn = false;
            if (null != _dragEventData)
            {
                OnEndDrag(_dragEventData);
            }
        }
    }

    public override void Rebuild(CanvasUpdate executing)
    {
        if (executing != CanvasUpdate.PostLayout)
        {
            return;
        }
        base.Rebuild(executing);

        if (Application.isPlaying && _isAutoInit)
        {
            Init();
        }
    }

    public PageView Init()
    {
        if (horizontal)
        {
            content.pivot = new Vector2(0, 0.5f);
        }
        else if (vertical)
        {
            content.pivot = new Vector2(0.5f, 1f);
        }
        _totalPage = content.childCount - 1;

        int i = 0;
        foreach(RectTransform child in content)
        {
            PageItem item = child.GetComponent<PageItem>();
            if (null == item)
            {
                item = child.gameObject.AddComponent<PageItem>();
            }
            item.index = i;
            ++i;
        }

        if (_pageIndicator)
        {
            _pageIndicator.Page = this;
            _pageIndicator.Build(_totalPage + 1);
        }

        _endPos = content.anchoredPosition;
        return this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isIn = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnEndDrag(eventData);
    }

    public bool PageIsLoop()
    {
        return _isLoop;
    }

    public void ShowPage(int index)
    {
        
    }

    public override void OnScroll(PointerEventData data)
    {
        
    }

    protected void CheckLoop(PointerEventData eventData)
    {
        if (horizontal)
        {
            if(eventData.delta.x > 0)
            {
                RectTransform child = content.GetChild(0) as RectTransform;
                if (child.anchoredPosition.x + content.anchoredPosition.x > -child.sizeDelta.x)
                {
                    RectTransform lastChild = content.GetChild(content.childCount - 1) as RectTransform;
                    Vector2 v = lastChild.anchoredPosition;
                    v.x = child.anchoredPosition.x - viewRect.sizeDelta.x;
                    lastChild.anchoredPosition = v;
                    lastChild.SetAsFirstSibling();
                }
            }
            else
            {
                RectTransform child = content.GetChild(content.childCount - 1) as RectTransform;
                if (child.anchoredPosition.x + content.anchoredPosition.x < child.sizeDelta.x)
                {
                    RectTransform firstChild = content.GetChild(0) as RectTransform;
                    Vector2 v = firstChild.anchoredPosition;
                    v.x = child.anchoredPosition.x + viewRect.sizeDelta.x;
                    firstChild.anchoredPosition = v;
                    firstChild.SetAsLastSibling();
                }
            }
        }
        else if (vertical)
        {
            if (eventData.delta.y < 0)
            {
                RectTransform child = content.GetChild(0) as RectTransform;
                if (child.anchoredPosition.y + content.anchoredPosition.y < child.sizeDelta.y)
                {
                    RectTransform lastChild = content.GetChild(content.childCount - 1) as RectTransform;
                    Vector2 v = lastChild.anchoredPosition;
                    v.y = child.anchoredPosition.y + viewRect.sizeDelta.y;
                    lastChild.anchoredPosition = v;
                    lastChild.SetAsFirstSibling();
                }
            }
            else
            {
                RectTransform child = content.GetChild(content.childCount - 1) as RectTransform;
                if (child.anchoredPosition.y + content.anchoredPosition.y > -child.sizeDelta.y)
                {
                    RectTransform firstChild = content.GetChild(0) as RectTransform;
                    Vector2 v = firstChild.anchoredPosition;
                    v.y = child.anchoredPosition.y - viewRect.sizeDelta.y;
                    firstChild.anchoredPosition = v;
                    firstChild.SetAsLastSibling();
                }
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.touchCount > 1)
        {
            return;
        }

        Graphic g = GetComponent<Graphic>();
        if (null == g || false == g.raycastTarget)
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        _isDrag = true;
        _isUpdateEnabled = true;
        _isIn = true;

        _contentPos = content.anchoredPosition;
        _dragTime = Time.realtimeSinceStartup;
        base.OnBeginDrag(eventData);
        if (null != _onBeginDragAction)
        {
            _onBeginDragAction.Invoke(eventData.delta);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (Input.touchCount > 1)
        {
            return;
        }
        if (!_isDragOutSideEnabled && !_isIn)
        {
            return;
        }
        _dragEventData = eventData;

        Graphic g = GetComponent<Graphic>();
        if (g && g.raycastTarget)
        {
            base.OnDrag(eventData);
        }
        if (_isLoop)
        {
            CheckLoop(eventData);
        }
        if (null != _onDragAction)
        {
            _onDragAction.Invoke(eventData.delta);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        Graphic g = GetComponent<Graphic>();
        if (null == g || false == g.raycastTarget)
        {
            return;
        }
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (!_isDrag)
        {
            return;
        }
        _isDrag = false;
        base.OnEndDrag(eventData);

        float dis = horizontal ? Mathf.Abs(eventData.pressPosition.x - eventData.position.x) : Mathf.Abs(eventData.pressPosition.y - eventData.position.y);
        if (Time.realtimeSinceStartup - _dragTime < 0.2f && dis > 50f)
        {
            if (horizontal)
            {
                if (eventData.position.x - eventData.pressPosition.x >= 0.5f)
                {
                    PrevPage();
                }
                else if (eventData.position.x - eventData.pressPosition.x <= -0.5f)
                {
                    NextPage();
                }
            }
            else if (vertical)
            {
                if (eventData.position.y - eventData.pressPosition.y >= 0.5f)
                {
                    NextPage();
                }
                else if (eventData.position.y - eventData.pressPosition.y <= -0.5f)
                {
                    PrevPage();
                }
            }
        }
        else
        {
            if (horizontal)
            {
                if (content.anchoredPosition.x - _contentPos.x > viewRect.sizeDelta.x / 2f)
                {
                    PrevPage();
                }
                else if (content.anchoredPosition.x - _contentPos.x < -viewRect.sizeDelta.x / 2f)
                {
                    NextPage();
                }
            }
            else if (vertical)
            {
                if (content.anchoredPosition.y - _contentPos.y > viewRect.sizeDelta.y / 2f)
                {
                    NextPage();
                }
                else if (content.anchoredPosition.y - _contentPos.y < -viewRect.sizeDelta.y / 2f)
                {
                    PrevPage();
                }
            }
        }
        if (null != _onEndDragAction)
        {
            _onEndDragAction.Invoke(eventData.delta);
        }
    }

    public void GotoPage(int pageIndex, bool anim = true)
    {
        _isDrag = false;
        if (_currentPage != pageIndex)
        {
            if (_isLoop)
            {
                if (pageIndex < 0)
                {
                    pageIndex = _totalPage;
                }
                else if (pageIndex > _totalPage)
                {
                    pageIndex = 0;
                }
            }
            else
            {
                if (pageIndex < 0)
                {
                    pageIndex = 0;
                }
                else if (pageIndex > _totalPage)
                {
                    pageIndex = _totalPage;
                }
            }

            _currentPage = pageIndex;

            RectTransform child = null;
            foreach(Transform temp in content)
            {
                PageItem item = temp.gameObject.GetComponent<PageItem>();
                if (item.index == pageIndex)
                {
                    child = temp as RectTransform;
                    break;
                }
            }
            if (null == child)
            {
                return;
            }

            Vector2 endPos = -child.anchoredPosition;
            _endPos = endPos;
            _isUpdateEnabled = true;
            _onPageChange.Invoke(_currentPage);
            if (_pageIndicator)
            {
                _pageIndicator.ShowPage(child.GetComponent<PageItem>().index);
            }

            if (!anim)
            {
                _isUpdateEnabled = false;
                SetContentAnchoredPosition(_endPos);
            }
        }
    }

    public bool HasPrevPage()
    {
        if (_isLoop && _totalPage > 1)
        {
            return true;
        }
        return _currentPage > 0;
    }

    public bool HasNextPage()
    {
        if (_isLoop && _totalPage > 1)
        {
            return true;
        }
        return _currentPage < _totalPage;
    }

    public void PrevPage()
    {
        if (HasPrevPage())
        {
            GotoPage(_currentPage - 1);
        }
    }

    public void NextPage()
    {
        if (HasNextPage())
        {
            GotoPage(_currentPage + 1);
        }
    }

    public void SetCheckToolDrag(BaseTool tool, bool isSet)
    {
        if (isSet)
        {
            tool._drag.DragValidCheckEvent += UGUIDrag_DragValidCheckEvent;
            EventTriggerListener.Get(tool.gameObject).OnUp += PageView_OnUp;
            EventTriggerListener.Get(tool.gameObject).OnDragging += PageView_OnDragging;
        }
        else
        {
            tool._drag.DragValidCheckEvent -= UGUIDrag_DragValidCheckEvent;
            EventTriggerListener.Get(tool.gameObject).OnUp -= PageView_OnUp;
            EventTriggerListener.Get(tool.gameObject).OnDragging -= PageView_OnDragging;
        }
    }

    private void PageView_OnDragging(GameObject go, PointerEventData eventData)
    {
        if (true == _isScrolling)
        {
            OnDrag(eventData);
        }
    }

    private void PageView_OnUp(GameObject go, PointerEventData eventData)
    {
        if (true == _isScrolling)
        {
            OnEndDrag(eventData);
        }
        _isScrolling = false;
        _hasStartDrag = false;
        _markBeginDragPos = Vector3.zero;
        _markDragPos = Vector3.zero;
    }

    private bool UGUIDrag_DragValidCheckEvent(PointerEventData eventData)
    {
        if (false == _hasStartDrag)
        {
            _hasStartDrag = true;
            _markBeginDragPos = eventData.position;
            _isScrolling = false;
            return false;
        }
        else
        {
            _markDragPos = eventData.position;
            float dx = _markDragPos.x - _markBeginDragPos.x;
            float dy = _markDragPos.y - _markBeginDragPos.y;
            if ((true == horizontal && Mathf.Abs(dx) > Mathf.Abs(dy)) || (true == vertical && Mathf.Abs(dy) > Mathf.Abs(dx)))
            {
                OnBeginDrag(eventData);
                _isScrolling = true;
                return false;
            }
            else
            {
                _isScrolling = false;
                return true;
            }
        }
    }
}
