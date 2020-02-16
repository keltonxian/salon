using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using PureMVC.Core;
using System;
using DG.Tweening;

public class UGUIDrag : Base, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public enum DragBackEffect
    {
        None, Immediately, TweenPosition, TweenScale, ScaleDestroy, FadeOutDestroy, Destroy, Keep,
    }
    public enum TriggerType
    {
        Point, Circle, Range,
    }

    private float _dragMoveDamp;
    private Vector3 _worldPos;
    public Vector3 DragTargetWorldPos
    {
        get
        {
            return _worldPos;
        }
        set
        {
            _worldPos = value;
        }
    }

    private Vector3 _touchDownTargetOffset;
    private Transform _prevParent;
    public Transform PrevParent
    {
        get
        {
            return _prevParent;
        }
    }
    private bool _isDown = false;
    private bool _isDragging = false;
    public bool IsDragging
    {
        get
        {
            return _isDragging && _isDown;
        }
    }
    private bool _canDrag = true;
    public bool CanDrag
    {
        get
        {
            return _canDrag;
        }
        set
        {
            _canDrag = value;
            if (!value)
            {
                _isDragging = false;
                _isDown = false;
            }
        }
    }

    public Vector3 OriginToTriggerOffset
    {
        get
        {
            return _tool.position - _triggerPos.position;
        }
    }

    [Tooltip("Target to drag, default is self")]
    public RectTransform _tool;
    private Image _toolImageNormal = null;
    private Image _toolImagePickup = null;
    private Image _toolImageShadow = null;
    // Cache Transform
    private Vector3 _toolDefaultPosition;
    private Vector3 _toolDefaultScale;
    private Vector3 _toolDefaultRotation;
    private int _toolDefaultIndex;
    private Vector3 _toolCachePosition;
    private Vector3 _toolCacheScale;
    private Vector3 _toolCacheRotation;
    private int _toolCacheIndex;
    // Box
    public RectTransform _box;
    private Image _boxImageNormal = null;
    private Image _boxImageSelected = null;
    private Image _boxImageShadow = null;

    [Tooltip("Default use main camera")]
    public Camera _raycastCamera = null;

    [Tooltip("Raycast layer")]
    public LayerMask _raycastMask;

    [Tooltip("Raycast depth")]
    public float _raycastDepth = 100f;

    [Header("Drag setting")]
    [Tooltip("Is fixed in taget origin point when drag")]
    public bool _isDragOriginPoint = false;

    [Tooltip("drag offset")]
    public Vector2 _dragOffset;

    [Tooltip("Affect show order(meter)")]
    public float _dragOffsetZ = 0f;

    [Tooltip("Affect target scale when drag")]
    public float _dragChangeScale = 1f;

    [Tooltip("Affect target rotation when drag")]
    public float _dragChangeRotate = 0f;

    [Tooltip("Parent when drag")]
    public Transform _draggingParent = null;

    [Tooltip("Is do drag when pointer down")]
    public bool _isDragOnPointerDown = true;

    [Tooltip("Trigger Position")]
    public Transform _triggerPos;

    [Tooltip("Trigger Type")]
    public TriggerType _triggerType = TriggerType.Point;

    [Tooltip("Trigger radius when type is circle")]
    public float _triggerRadius = 1f;

    [Tooltip("Trigger area when type is range")]
    public Vector2 _triggerRange = Vector2.one;

    [Header("Event")]
    private bool _isSendHoverEvent = false;
    public bool IsSendHoverEvent
    {
        get
        {
            return _isSendHoverEvent;
        }
        set
        {
            _isSendHoverEvent = value;
        }
    }

    [Header("Drag Effect")]
    public AudioClip _soundDragging;
    private string _soundDraggingID;
    public ParticleSystem _particleDragging;
    public enum DRAG_EFFECT_TYPE
    {
        NONE, ON_DRAG, IN_AREA,
    }
    public DRAG_EFFECT_TYPE _dragEffectType = DRAG_EFFECT_TYPE.NONE;
    public enum DRAG_VALID_TYPE
    {
        NONE, HORIZONTAL, VERTICAL,
    }
    [Header("Check drag valid")]
    public DRAG_VALID_TYPE _dragValidType = DRAG_VALID_TYPE.NONE;
    [Header("Page View")]
    public PageView _pageView;

    [Header("Back bffect")]
    [Tooltip("Is auto back when release")]
    public bool _isReleaseAutoBack = false;
    [Tooltip("Back effect")]
    public DragBackEffect _backEffect = DragBackEffect.None;
    [Tooltip("Back time")]
    public float _backDuring = 0.5f;
    [Tooltip("Tween effect")]
    public Ease _tweenEase = Ease.Linear;
    [Tooltip("Is keep on top when back")]
    public bool _isBackKeepTop = true;
    private bool _hasStartBackKeepTop = false;
    private Vector3 _markDragBackPos;
    private Vector3 _markDragBackScale;
    private UGUIDrag.DragBackEffect _markDragBackEffect;
    private float _markDragBackDuring;

    [Header("Drag Area Restrict")]
    private bool _isRestrict = false;
    private Rect _dragArea = new Rect(-3.84f, -3f, 7.68f, 8f);

    [Header("Tip")]
    public bool _hasTip = false;
    public GameObject _tipBeforeDrag;
    public GameObject _tipDragging;
    // Click Anim
    [Header("Click")]
    public bool _hasBtn = false;
    public UIButton _btnClick;
    public AudioClip _soundClick;
    private bool _isClickAnimEnabled = true;
    private float _jumpHeight = 25.0f;
    private bool _isClickRotateEnabled = false;
    private float _rotateAngle = 25.0f;
    // Edge Flip
    private bool _isCanEdgeFlip = false;
    private float _edgeDistance = 20.0f;
    [Header("Audio")]
    public bool _hasAudio = false;
    public AudioClip _soundStartDrag;
    public AudioClip _soundEndDrag;
    // VO
    public AudioClip _voStartDrag;
    private int _voStartDragPlayTimes = 1;
    private bool _voStartDragPlayed = false;
    public AudioClip _voEndDrag;
    private int _voEndDragPlayTimes = 1;
    private bool _voEndDragPlayed = false;
    public AudioClip _voDone;
    //private int _voDonePlayTimes = 1;
    //private bool _voDonePlayed = false;

    public event Action<UGUIDrag, PointerEventData> OnPrevBeginDragAction = null;
    public event Action<UGUIDrag, PointerEventData> OnBeginDragAction = null;
    public event Action<UGUIDrag, PointerEventData> OnDragAction = null;
    public event Action<UGUIDrag> OnDragTargetMoveAction = null;
    public event Action<UGUIDrag, PointerEventData> OnEndDragAction = null;
    public event Action<UGUIDrag, PointerEventData> OnPrevEndDragAction = null;
    public event Action<UGUIDrag> OnTweenStartAction = null, OnTweenOverAction = null;
    public delegate bool DragValidCheck(PointerEventData eventData);
    public event DragValidCheck DragValidCheckEvent;

    public event Action<UGUIDrag, Collider2D[]> OnHoverColliderAction;
    public event Action<UGUIDrag, Collider2D[]> OnDropColliderAction;
    public event Action<UGUIDrag> OnHoverColliderOutAction;

    public event Action<UGUIDrag> OnClickAction;

    void OnEnable()
    {
        _isDown = false;
        _isDragging = false;
    }

    void OnDisable()
    {
        _isDown = false;
        _isDragging = false;
    }

    public void SetEnabled(bool isEnabled)
    {
        CanDrag = isEnabled;
        SetTouchEnabled(isEnabled);
    }

    public void SetTouchEnabled(bool isEnabled)
    {
        transform.GetComponent<Graphic>().raycastTarget = isEnabled;
    }

    public bool GetTouchEnabled()
    {
        return transform.GetComponent<Graphic>().raycastTarget;
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        InitToolData();
        InitToolImageState();
        InitBoxImageState();

        _pageView = transform.GetComponentInParent<PageView>();

        if (null != _btnClick)
        {
            _btnClick.OnClick.AddListener(OnClickEvent);
        }

        if (!_raycastCamera)
        {
            _raycastCamera = Camera.main;
        }

        if (true == _isBackKeepTop)
        {
            _markDragBackEffect = _backEffect;
            _markDragBackDuring = _backDuring;
        }

        DragValidCheckEvent += OnDragValidCheck;
    }

    private void InitToolData()
    {
        if (null == _tool)
        {
            Transform t = transform.Find("Target");
            if (null != t)
            {
                Transform tt = t.Find("Tool");
                if (null != tt)
                {
                    _tool = tt.GetComponent<RectTransform>();
                }
            }
            if (null == _tool)
            {
                _tool = transform.GetComponent<RectTransform>();
            }
        }

        if (null == _tool)
        {
            return;
        }

        _toolDefaultScale = _tool.localScale;
        _toolDefaultRotation = _tool.localEulerAngles;
        _toolDefaultPosition = _tool.localPosition;
        _toolDefaultIndex = _tool.GetSiblingIndex();

        if (null == _particleDragging)
        {
            Transform particle = _tool.Find("ParticleDragging");
            if (null != particle)
            {
                _particleDragging = particle.GetComponent<ParticleSystem>();
            }
        }

        if (!_triggerPos)
        {
            _triggerPos = _tool;
        }
    }

    public void SetDragEffectPlay(bool isPlay)
    {
        SetParticleDraggingPlay(isPlay);
        SetSoundDragPlay(isPlay);
    }

    private void SetParticleDraggingPlay(bool isPlay)
    {
        if (null == _particleDragging)
        {
            return;
        }
        if (isPlay == _particleDragging.isPlaying)
        {
            return;
        }
        if (isPlay)
        {
            _particleDragging.Play();
        }
        else
        {
            _particleDragging.Stop();
        }
    }

    private void SetSoundDragPlay(bool isPlay)
    {
        if (true == isPlay && null != _soundDragging && string.IsNullOrEmpty(_soundDraggingID))
        {
            _soundDraggingID = AudioManager.PlayMusic(_soundDragging);
        }
        else if (false == isPlay && !string.IsNullOrEmpty(_soundDraggingID))
        {
            AudioManager.StopMusicByGUID(_soundDraggingID);
            _soundDraggingID = null;
        }
    }

    private void InitToolImageState()
    {
        if (null == _tool)
        {
            return;
        }
        if (null == _toolImageNormal)
        {
            Transform t = _tool.Find("Normal");
            if (null != t)
            {
                _toolImageNormal = t.GetComponent<Image>();
            }
        }
        if (null == _toolImagePickup)
        {
            Transform t = _tool.Find("Pickup");
            if (null != t)
            {
                _toolImagePickup = t.GetComponent<Image>();
            }
        }
        if (null == _toolImageShadow)
        {
            Transform t = _tool.Find("Shadow");
            if (null != t)
            {
                _toolImageShadow = t.GetComponent<Image>();
            }
        }
        SetToolState(false);
    }

    private void SetToolState(bool isPickup)
    {
        if (null == _toolImagePickup)
        {
            return;
        }
        if (null != _toolImageNormal)
        {
            _toolImageNormal.enabled = !isPickup;
        }
        if (null != _toolImagePickup)
        {
            _toolImagePickup.enabled = isPickup;
        }
        if (null != _toolImageShadow)
        {
            _toolImageShadow.DOKill();
            if (isPickup)
            {
                _toolImageShadow.DOFade(0, 0.1f);
            }
            else
            {
                _toolImageShadow.DOFade(0, 0.5f);
            }
        }
    }

    private void InitBoxImageState()
    {
        if (null == _box)
        {
            return;
        }
        if (null == _boxImageNormal)
        {
            _boxImageNormal = _box.Find("Normal").GetComponent<Image>();
        }
        if (null == _boxImageSelected)
        {
            _boxImageSelected = _box.Find("Selected").GetComponent<Image>();
        }
        if (null == _boxImageShadow)
        {
            _boxImageShadow = _box.Find("Shadow").GetComponent<Image>();
        }
        SetBoxState(false);
    }

    private void SetBoxState(bool isPickup)
    {
        if (null == _toolImagePickup)
        {
            return;
        }
        if (null != _toolImageNormal)
        {
            _toolImageNormal.enabled = !isPickup;
        }
        if (null != _toolImagePickup)
        {
            _toolImagePickup.enabled = isPickup;
        }
        if (null != _toolImageShadow)
        {
            _toolImageShadow.DOKill();
            if (isPickup)
            {
                _toolImageShadow.DOFade(0, 0.1f);
            }
            else
            {
                _toolImageShadow.DOFade(0, 0.5f);
            }
        }
    }

    public void InitStatus()
    {

    }

    public void ResetStatus()
    {
        SetToolState(false);
        SetBoxState(false);
    }

    public void UnSelected()
    {
        SetToolState(false);
        SetBoxState(false);
    }

    private bool OnDragValidCheck(PointerEventData eventData)
    {
        Vector2 pos1 = eventData.pressPosition;
        Vector2 pos2 = eventData.position;
        float rad2deg = 180f / Mathf.PI;
        float targetAngle = -Mathf.Atan2(pos1.y - pos2.y, pos1.x - pos2.x) * rad2deg;
        if (_dragValidType == DRAG_VALID_TYPE.HORIZONTAL)
        {
            if (eventData.delta.y > 0 && targetAngle > 30 && targetAngle < 150)
            {
                return true;
            }
        }
        else
        {
            if (eventData.delta.x > 0 && ((targetAngle > 90 && targetAngle <= 180) || (targetAngle >= -180 && targetAngle < -90)))
            {
                return true;
            }
        }
        if (eventData.delta.y > 0 && targetAngle > 30 && targetAngle < 150)
        {
            return true;
        }
        return false;
    }

    void OnApplicationFocus(bool flag)
    {
        if (!flag && _canDrag && _isDragging)
        {
            OnEndDrag(null);
        }
    }

    void Update()
    {
        if (!this.enabled)
        {
            return;
        }
        if (_canDrag && _isDragging)
        {
            if (_dragMoveDamp < 1f)
            {
                _dragMoveDamp += 0.01f;
            }
            _tool.position = Vector3.Lerp(_tool.position, _worldPos, _dragMoveDamp);
            if (Vector2.Distance((Vector2)_tool.position, (Vector2)_worldPos) > 0.001f)
            {
                if (null != OnDragTargetMoveAction)
                {
                    OnDragTargetMoveAction.Invoke(this);
                }
            }
        }
    }

    public void SetDefaultPosition()
    {
        if (_tool)
        {
            _tool.localPosition = _toolDefaultPosition;
        }
    }

    public void SetDefaultScale()
    {
        if (_tool)
        {
            _tool.localScale = _toolDefaultScale;
        }
    }

    public void SetDefaultRotation()
    {
        if (_tool)
        {
            _tool.localEulerAngles = _toolDefaultRotation;
        }
    }

    public void SetDefaultIndex()
    {
        if (_tool)
        {
            _tool.SetSiblingIndex(_toolDefaultIndex);
        }
    }

    public void OnClickEvent(PointerEventData eventData)
    {
        if (false == eventData.dragging || (eventData.delta.magnitude < 1.1f && Vector2.Distance(eventData.pressPosition, eventData.position) < 12))
        {
            if (true == this.enabled)
            {
                if (true == _isClickAnimEnabled)
                {
                    PlayJumpAnim();
                }
            }
            if (null != _soundClick)
            {
                AudioManager.PlaySound(_soundClick);
            }

            if (null != _pageView)
            {
                _pageView.OnEndDrag(eventData);
            }
            SetBoxState(true);

            OnClickAction.Invoke(this);
        }
    }

    protected void PlayJumpAnim()
    {
        if (null != _tool)
        {
            this.enabled = false;
            RectTransform target = _tool;
            float y = _toolCachePosition.y;
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOLocalMoveY(y + _jumpHeight, 0.1f));
            seq.Append(target.DOLocalMoveY(y, 0.1f));
            seq.Append(target.DOLocalMoveY(y + _jumpHeight / 2, 0.1f));
            seq.Append(target.DOLocalMoveY(y + _jumpHeight, 0.1f));
            seq.AppendCallback(() =>
            {
                this.enabled = true;
            });
        }
        if (null != _toolImageShadow)
        {
            Image image = _toolImageShadow;
            Sequence seq = DOTween.Sequence();
            seq.Append(image.DOFade(0.2f, 0.1f));
            seq.Append(image.DOFade(1.0f, 0.1f));
            seq.Append(image.DOFade(0.2f, 0.1f));
            seq.Append(image.DOFade(1.0f, 0.1f));
        }
    }

    protected void PlayRotateAnim()
    {
        if (false == _isClickRotateEnabled)
        {
            return;
        }
        if (null == _tool)
        {
            return;
        }
        RectTransform target = _tool;
        target.DOKill();
        target.DOLocalMoveY(_toolCachePosition.y + _jumpHeight, 0.5f);
        target.DOLocalRotate(new Vector3(0f, 0f, _rotateAngle), 0.25f, RotateMode.Fast).OnComplete(() =>
        {
            target.DOLocalRotate(new Vector3(0f, 0f, -_rotateAngle), 0.5f, RotateMode.Fast).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        });
    }

    public void ResetAnim()
    {
        if (null != _tool)
        {
            RectTransform target = _tool;
            target.DOKill();
            target.DOLocalMove(_toolCachePosition, 0.5f);
            target.DOLocalRotate(_toolCacheRotation, 0.25f);
            target.DOScale(_toolCacheScale, 0.5f);
        }
    }

    private void DragAreaRestrict()
    {
        if (true == _isRestrict && true == _isDragging)
        {
            Vector3 pos = DragTargetWorldPos;
            Rect area = _dragArea;
            if (pos.x < area.xMin)
            {
                pos.x = area.xMin;
            }
            else if (pos.x > area.xMax)
            {
                pos.x = area.xMax;
            }
            else if (pos.y < area.yMin)
            {
                pos.y = area.yMin;
            }
            else if (pos.y > area.yMax)
            {
                pos.y = area.yMax;
            }
        }
    }

    private void FlipTool()
    {
        if (true == _isCanEdgeFlip)
        {
            Vector3 scale = _toolCacheScale;
            RectTransform target = _tool;
            if (Input.mousePosition.x < _edgeDistance)
            {
                target.localScale = new Vector3(-scale.x, scale.y, scale.z);
            }
            else if (Input.mousePosition.x > Screen.width - _edgeDistance)
            {
                target.localScale = scale;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!this.enabled || _isDown)
        {
            return;
        }

        if (null != DragValidCheckEvent)
        {
            if (!DragValidCheckEvent(eventData))
            {
                _canDrag = false;
                return;
            }
        }
        if (null != OnPrevBeginDragAction)
        {
            OnPrevBeginDragAction.Invoke(this, eventData);
        }

        _dragMoveDamp = 0.3f;
        _isDown = true;
        _canDrag = true;
        _tool.DOKill();

        _isDragging = true;
        GetComponent<Graphic>().raycastTarget = false;
        _toolCachePosition = _tool.localPosition;
        _toolCacheScale = _tool.localScale;
        _toolCacheRotation = _tool.localEulerAngles;
        _toolCacheIndex = _tool.GetSiblingIndex();

        if (Math.Abs(_dragChangeScale) > float.Epsilon)
        {
            _tool.DOScale(_toolCacheScale * _dragChangeScale, 0.4f);
        }
        if (Math.Abs(_dragChangeRotate) > float.Epsilon)
        {
            _tool.DOLocalRotate(_toolCacheRotation + new Vector3(0f, 0f, _dragChangeRotate), 0.4f, RotateMode.Fast);
        }

        _tool.position += new Vector3(0, 0, _dragOffsetZ);
        _worldPos = _tool.position;
        Vector3 touchDownMousePos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_tool, eventData.position, _raycastCamera, out touchDownMousePos);
        _touchDownTargetOffset = _worldPos - touchDownMousePos;
        if (_isDragOriginPoint)
        {
            _worldPos += _touchDownTargetOffset * 0.01f;
        }

        _prevParent = _tool.parent;

        if (_draggingParent)
        {
            _tool.SetParent(_draggingParent);
        }

        if (true == _isBackKeepTop && false == _hasStartBackKeepTop)
        {
            _hasStartBackKeepTop = true;
            _backEffect = UGUIDrag.DragBackEffect.Keep;
            _markDragBackPos = _tool.position;
            _markDragBackScale = _tool.localScale;
        }

        SetToolState(true);
        SetBoxState(true);

        if (null != _tipBeforeDrag)
        {
            Destroy(_tipBeforeDrag);
        }
        if (null != _tipDragging)
        {
            _tipDragging.SetActive(true);
        }

        PlaySoundStartDrag();

        if (_dragEffectType == DRAG_EFFECT_TYPE.ON_DRAG)
        {
            SetDragEffectPlay(true);
        }

        if (null != OnBeginDragAction)
        {
            OnBeginDragAction.Invoke(this, eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!this.enabled || !_canDrag || !_isDown)
        {
            return;
        }

        if (_draggingParent)
        {
            if (_tool.parent != _draggingParent)
            {
                _tool.SetParent(_draggingParent);
            }
        }

        if (true == eventData.dragging)
        {
            _isDragging = true;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_tool, eventData.position, _raycastCamera, out _worldPos);
            if (!_isDragOriginPoint)
            {
                _worldPos += _touchDownTargetOffset;
            }
            _worldPos += (Vector3)_dragOffset * 0.01f;
            CheckHoverEvent(false);
        }

        if (null != OnDragAction)
        {
            OnDragAction.Invoke(this, eventData);
        }

        DragAreaRestrict();
        FlipTool();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!this.enabled || !_canDrag || !_isDown)
        {
            return;
        }
        _isDragging = false;
        _isDown = false;

        if (null != OnPrevEndDragAction)
        {
            OnPrevEndDragAction.Invoke(this, eventData);
        }

        DOTween.Kill(_tool);
        if (Math.Abs(_dragChangeScale) > float.Epsilon)
        {
            _tool.DOScale(_toolCacheScale, 0.25f);
        }
        if (Math.Abs(_dragChangeRotate) > float.Epsilon)
        {
            _tool.DOLocalRotate(_toolCacheRotation, 0.25f, RotateMode.Fast);
        }

        CheckHoverEvent(true);

        if (_isReleaseAutoBack)
        {
            BackPosition();
        }
        else
        {
            _tool.position -= new Vector3(0, 0, _dragOffsetZ);
            if (_prevParent)
            {
                _tool.SetParent(_prevParent);
                _tool.SetSiblingIndex(_toolCacheIndex);
                _canDrag = true;
            }
        }

        if (null != _pageView)
        {
            _pageView.GotoPage(_pageView.CurrentPage);
        }
        if (null != _tipDragging)
        {
            _tipDragging.SetActive(false);
        }

        PlaySoundEndDrag();

        SetDragEffectPlay(false);

        if (null != OnEndDragAction)
        {
            OnEndDragAction.Invoke(this, eventData);
        }
        GetComponent<Graphic>().raycastTarget = true;
    }

    public void PlaySoundStartDrag()
    {
        if (null != _soundStartDrag)
        {
            AudioManager.PlaySound(_soundStartDrag);
        }
        if (null != _voStartDrag && false == _voStartDragPlayed)
        {
            _voStartDragPlayTimes -= 1;
            if (_voStartDragPlayTimes < 1)
            {
                _voStartDragPlayed = true;
            }
            AudioManager.PlaySound(_voStartDrag);
        }
    }

    public void PlaySoundEndDrag()
    {
        if (null != _soundEndDrag)
        {
            AudioManager.PlaySound(_soundEndDrag);
        }
        if (null != _voEndDrag && false == _voEndDragPlayed)
        {
            _voEndDragPlayTimes -= _voEndDragPlayTimes;
            if (_voEndDragPlayTimes < 1)
            {
                _voEndDragPlayed = true;
            }
            AudioManager.PlaySound(_voEndDrag);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragOnPointerDown)
        {
            if (null != DragValidCheckEvent)
            {
                if (!DragValidCheckEvent(eventData))
                {
                    return;
                }
            }
            OnBeginDrag(eventData);
            eventData.dragging = true;
            OnDrag(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnEndDrag(eventData);
    }

    private void CheckHoverEvent(bool isDrop = false)
    {
        if (!IsSendHoverEvent)
        {
            return;
        }
        Collider2D[] cols = null;
        if (_triggerType == TriggerType.Point)
        {
            cols = Physics2D.OverlapPointAll(_triggerPos.position, _raycastMask, -_raycastDepth, _raycastDepth);
        }
        else if (_triggerType == TriggerType.Circle)
        {
            cols = Physics2D.OverlapCircleAll(_triggerPos.position, _triggerRadius, _raycastMask, -_raycastDepth, _raycastDepth);
        }
        else if (_triggerType == TriggerType.Range)
        {
            cols = Physics2D.OverlapBoxAll(_triggerPos.position, _triggerRange * 2f, _triggerPos.eulerAngles.z, _raycastMask, -_raycastDepth, _raycastDepth);
        }
        if (true == isDrop)
        {
            if (null != cols && cols.Length > 0)
            {
                if (null != OnDropColliderAction)
                {
                    OnDropColliderAction.Invoke(this, cols);
                }
            }
        }
        else
        {
            if (null != cols && cols.Length > 0)
            {
                if (null != OnHoverColliderAction)
                {
                    OnHoverColliderAction.Invoke(this, cols);
                }
            }
            else
            {
                if (null != OnHoverColliderOutAction)
                {
                    OnHoverColliderOutAction.Invoke(this);
                }
            }
        }
    }

    public void OnTweenOver()
    {
        SetDragEffectPlay(false);
        ResetStatus();
        if (null != OnTweenOverAction)
        {
            OnTweenOverAction.Invoke(this);
        }
    }

    public void BackPosition(UGUIDrag.DragBackEffect backEffect = UGUIDrag.DragBackEffect.None)
    {
        if (_isBackKeepTop)
        {
            BackOnTop(backEffect);
        }
        else
        {
            DragBackEffect mark = _backEffect;
            _backEffect = backEffect;
            BackPositionAction();
            _backEffect = mark;
        }
    }

    private void BackOnTop(UGUIDrag.DragBackEffect backEffect = UGUIDrag.DragBackEffect.None)
    {
        if (false == _isBackKeepTop || false == _hasStartBackKeepTop)
        {
            return;
        }
        _hasStartBackKeepTop = false;
        this.enabled = false;
        _backDuring = 0.1f;
        RectTransform target = _tool;
        if (backEffect == UGUIDrag.DragBackEffect.TweenScale)
        {
            target.position = _markDragBackPos;
            target.localScale = Vector3.zero;
            target.DOScale(_markDragBackScale, _markDragBackDuring).SetEase(Ease.Linear).OnComplete(() =>
            {
                BackOnTopCallback();
            });
        }
        else
        {
            target.DOScale(_markDragBackScale, _markDragBackDuring).SetEase(Ease.Linear);
            target.DOMove(_markDragBackPos, _markDragBackDuring).SetEase(Ease.Linear).OnComplete(() =>
            {
                BackOnTopCallback();
            });
        }
    }

    private void BackOnTopCallback()
    {
        _backEffect = UGUIDrag.DragBackEffect.Immediately;
        BackPositionAction();
        _backEffect = _markDragBackEffect;
        this.enabled = true;
        _backDuring = _markDragBackDuring;
        OnTweenOver();
    }

    public void BackPositionAction()
    {
        if (_backEffect == DragBackEffect.Keep)
        {
            return;
        }

        if (null != OnTweenStartAction)
        {
            OnTweenStartAction.Invoke(this);
        }

        switch (_backEffect)
        {
            case DragBackEffect.Destroy:
                Destroy(_tool.gameObject);
                break;
            case DragBackEffect.TweenPosition:
                this.enabled = false;
                _canDrag = false;
                if (_prevParent)
                {
                    _tool.SetParent(_prevParent);
                }
                _tool.DOLocalRotate(_toolCacheRotation, _backDuring).SetEase(_tweenEase);
                _tool.DOScale(_toolCacheScale, _backDuring).SetEase(_tweenEase);

                Canvas canvas = BackKeepTop();
                _tool.DOLocalMove(_toolCachePosition, _backDuring).SetEase(_tweenEase).OnComplete(() =>
                {
                    if (canvas && _tool.GetComponent<GraphicRaycaster>() == null)
                    {
                        Destroy(canvas);
                    }
                    this.enabled = true;
                    _canDrag = true;
                    _tool.SetSiblingIndex(_toolCacheIndex);
                    OnTweenOver();
                });
                break;
            case DragBackEffect.TweenScale:
                this.enabled = false;
                _canDrag = false;
                if (_prevParent)
                {
                    _tool.SetParent(_prevParent);
                }
                _tool.localPosition = _toolCachePosition;
                _tool.localScale = Vector3.zero;
                _tool.localEulerAngles = _toolCacheRotation;

                canvas = BackKeepTop();
                _tool.DOScale(_toolCacheScale, _backDuring).SetEase(_tweenEase).OnComplete(() =>
                {
                    if (canvas && _tool.GetComponent<GraphicRaycaster>() == null)
                    {
                        Destroy(canvas);
                    }
                    this.enabled = true;
                    _canDrag = true;
                    _tool.SetSiblingIndex(_toolCacheIndex);
                    OnTweenOver();
                });
                break;
            case DragBackEffect.ScaleDestroy:
                this.enabled = false;
                _canDrag = false;
                _tool.DOScale(Vector3.zero, _backDuring).SetEase(_tweenEase).OnComplete(() =>
                {
                    Destroy(_tool.gameObject);
                    OnTweenOver();
                });
                break;
            case DragBackEffect.FadeOutDestroy:
                this.enabled = false;
                _canDrag = false;
                CanvasGroup group = _tool.gameObject.AddComponent<CanvasGroup>();
                group.blocksRaycasts = false;
                group.DOFade(0f, _backDuring).SetEase(_tweenEase).OnComplete(() =>
                {
                    Destroy(_tool.gameObject);
                    OnTweenOver();
                });
                break;
            default:
                if (_prevParent)
                {
                    _tool.SetParent(_prevParent);
                }
                _tool.localPosition = _toolCachePosition;
                _tool.localScale = _toolCacheScale;
                _tool.localEulerAngles = _toolCacheRotation;
                _tool.SetSiblingIndex(_toolCacheIndex);
                _canDrag = true;
                _isDown = false;
                _isDragging = false;
                Canvas canvas2 = _tool.gameObject.GetComponent<Canvas>();
                if (canvas2 && _tool.GetComponent<GraphicRaycaster>() == null)
                {
                    Destroy(canvas2);
                }
                break;
        }
    }

    protected Canvas BackKeepTop()
    {
        Canvas canvas = null;
        if (_isBackKeepTop)
        {
            canvas = _tool.gameObject.GetComponent<Canvas>();
            if (null == canvas)
            {
                canvas = _tool.gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            Canvas rootCanvas = canvas.rootCanvas;
            if (rootCanvas)
            {
                canvas.sortingLayerID = rootCanvas.sortingLayerID;
                canvas.sortingOrder = rootCanvas.sortingOrder + 1;
            }
        }
        return canvas;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Transform origin = _triggerPos == null ? (_tool == null ? transform : _tool) : _triggerPos;
        if (_triggerType == TriggerType.Point)
        {
            Gizmos.DrawSphere(origin.position, 0.02f);
        }
        else if (_triggerType == TriggerType.Circle)
        {
            Gizmos.DrawWireSphere(origin.position, _triggerRadius);
        }
        else if (_triggerType == TriggerType.Range)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, (Vector3)_triggerRange * 2f);
            Gizmos.matrix = mat;
        }
    }
#endif
}
