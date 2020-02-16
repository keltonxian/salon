using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PageIndicator : UIBehaviour
{
    public Sprite _defaultState;
    public Sprite _selectedState;
    public bool _isInteractive = true;
    public float _space = 0f;
    public Color _color = Color.white;

    private float _pageItemWidth = 0f;
    private float _pageItemHeight = 0f;

    public Material _material;

    private int _prevPage = 0;
    private RectTransform _layer;

    private IPage _page = null;
    public IPage Page
    {
        set
        {
            _page = value;
        }
    }

    public void Build(int count)
    {
        if (_defaultState && _selectedState)
        {
            _pageItemWidth = _defaultState.bounds.size.x * 100f;
            _pageItemHeight = _defaultState.bounds.size.y * 100f;

            float totalWidth = (_pageItemWidth + _space) * (count - 1);

            for (int i = 0; i < count; i++)
            {
                Image img = (new GameObject()).AddComponent<Image>();
                img.color = _color;
                if (_isInteractive)
                {
                    EventTriggerListener.Get(img.gameObject).OnClick += OnPageItemClick;
                }
                if (i != _prevPage)
                {
                    img.sprite = _defaultState;
                }
                else
                {
                    img.sprite = _selectedState;
                }
                if (null != _material)
                {
                    img.material = _material;
                }
                RectTransform rt = img.transform.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(_pageItemWidth, _pageItemHeight);
                float size = _pageItemWidth;
                rt.SetParent(transform);
                rt.name = "PageItem";
                rt.localScale = Vector3.one;
                rt.anchoredPosition = new Vector2((size + _space) * i - totalWidth / 2f, 0);
            }
        }
    }

    public void ShowPage(int pageIndex)
    {
        if (_prevPage != pageIndex)
        {
            transform.GetChild(_prevPage).GetComponent<Image>().sprite = _defaultState;
            transform.GetChild(pageIndex).GetComponent<Image>().sprite = _selectedState;
            _prevPage = pageIndex;
        }
    }

    private void OnPageItemClick(GameObject go, PointerEventData eventData)
    {
        if (null != _page)
        {
            if (Input.touchCount <= 1)
            {
                _page.ShowPage(go.transform.GetSiblingIndex());
            }
        }
    }
}
