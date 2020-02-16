using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using System.Linq;

public class ScribbleTool : BaseTool
{
    public enum Mode
    {
        MoveFinger = 1, DragTool = 2,
    }

    public enum Operation
    {
        Scribble = 1, Erase = 2, Replace = 3, ReplaceErase = 4, DrawLine = 5, CutShape = 6,
    }

    [SerializeField]
    protected Callback.UnityEventV _onCutShape = new Callback.UnityEventV();
    public Callback.UnityEventV OnCutShape
    {
        get
        {
            return _onCutShape;
        }
    }

    public bool _isCheckOnUI = false;
    public Mode _mode = Mode.MoveFinger;
    public Operation _operation = Operation.Scribble;
    public RenderTexturePainter[] _arrayPainter;
    private List<RenderTexturePainter> _listPainter = new List<RenderTexturePainter>();
    public List<RenderTexturePainter> ListPainter
    {
        get
        {
            return _listPainter;
        }
    }
    public PainterChecker[] _arrayChecker;
    private List<PainterChecker> _listChecker = new List<PainterChecker>();
    public List<PainterChecker> ListChecker
    {
        get
        {
            return _listChecker;
        }
    }
    private Dictionary<RenderTexturePainter, int> _dicCheckerIndex = new Dictionary<RenderTexturePainter, int>();
    public Dictionary<RenderTexturePainter, int> DicCheckerIndex
    {
        get
        {
            return _dicCheckerIndex;
        }
    }
    // Pen Texture
    public bool _isUsePenTex = false;
    public Texture _penTex;
    public float _penScale = 1f;
    // Replace Erase Texture
    public Texture2D _replaceEraseTex = null;
    // Replace Texture
    public bool _isUseLastTex = false;
    public Texture2D _lastTex = null;
    // Use Finger Move
    private bool _hasStartDraw = false;
    private bool _isFingerMoved = false;
    public Image _finger;
    public bool _isHideFingerOnMove = false;
    private Vector3 _cacheFingerPos;
    private Vector3 _mousePressPos;
    private bool _isStopFingerTouch = false;
    // Scribble Attachment
    public bool _hasAttachment = false;
    public GameObject _prefabAttachment = null;
    private Dictionary<string, Transform> _dicAttachment = new Dictionary<string, Transform>();
    public Dictionary<string, Transform> DicAttachment
    {
        get
        {
            return _dicAttachment;
        }
    }
    private float _attachmentRandomOffset = 0.25f;
    private float _attachmentRandomScale = 0.5f;
    public AudioClip _soundAddAttachment;
    public AudioClip _soundRemoveAttachment;
    // Checker
    [Range(0, 1)]
    public float _checkerTargetPercentage = 1f;
    private bool _hasSendPainterFinish = false;
    // Operation Scribble
    private float _checkDataGridFactor = 0.02f;
    // Operation Erase
    private float _eraseRadius = 0.2f;
    public bool _isCleanOtherPaintTool = false;
    public ScribbleTool[] _arrayScribbleTool;
    private List<ScribbleTool> _listScribbleTool = new List<ScribbleTool>();
    public List<ScribbleTool> ListScribbleTool
    {
        get
        {
            return _listScribbleTool;
        }
    }
    //
    private bool _hasDoEndDrawActions = false;
    [SerializeField]
    protected Callback.UnityEventF _onPainterPercentage = new Callback.UnityEventF();
    public Callback.UnityEventF OnPainterPercentage
    {
        get
        {
            return _onPainterPercentage;
        }
    }
    [SerializeField]
    protected Callback.UnityEventV _onPainterFinish = new Callback.UnityEventV();
    public Callback.UnityEventV OnPainterFinish
    {
        get
        {
            return _onPainterFinish;
        }
    }
    
    [SerializeField]
    protected Callback.UnityEventB _onInArea = new Callback.UnityEventB();
    public Callback.UnityEventB OnInArea
    {
        get
        {
            return _onInArea;
        }
    }

    // Use With ColorHandler
    public Texture[] _arrayDrawMainTex;

    private Coroutine _coOnAreaOut;
    private Coroutine _coFadeCanvas;

    private delegate void CallbackLoopPainter(RenderTexturePainter painter);
    private void LoopPainter(CallbackLoopPainter callback)
    {
        for (int i = 0; i < _listPainter.Count; i++)
        {
            RenderTexturePainter painter = _listPainter[i];
            callback.Invoke(painter);
        }
    }
    private delegate void CallbackLoopChecker(PainterChecker checker);
    private void LoopChecker(CallbackLoopChecker callback)
    {
        for (int i = 0; i < _listChecker.Count; i++)
        {
            PainterChecker checker = _listChecker[i];
            callback.Invoke(checker);
        }
    }
    private delegate void CallbackLoopScribbleTool(ScribbleTool tool);
    private void LoopScribbleTool(CallbackLoopScribbleTool callback)
    {
        for (int i = 0; i < _listScribbleTool.Count; i++)
        {
            ScribbleTool tool = _listScribbleTool[i];
            callback.Invoke(tool);
        }
    }

    public override void Init()
    {
        base.Init();

        SetupArgs();
    }

    public void Init(RenderTexturePainter[] arrayPainter, PainterChecker[] arrayChecker)
    {
        base.Init();

        SetupArgs(arrayPainter, arrayChecker);
    }

    private void SetupArgs(RenderTexturePainter[] arrayPainter = null, PainterChecker[] arrayChecker = null)
    {
        InitPainterAndChecker(arrayPainter, arrayChecker);

        if (_mode == Mode.MoveFinger && null != _finger)
        {
            _cacheFingerPos = _finger.transform.localPosition;
        }
    }

    private void InitPainterAndChecker(RenderTexturePainter[] arrayPainter = null, PainterChecker[] arrayChecker = null)
    {
        _listPainter.Clear();

        RenderTexturePainter[] arrayTempPainter = _arrayPainter;
        if (null != arrayPainter)
        {
            arrayTempPainter = _arrayPainter.Concat(arrayPainter).ToArray();
        }
        for (int i = 0; i < arrayTempPainter.Length; i++)
        {
            _listPainter.Add(arrayTempPainter[i]);
        }

        if (_operation == Operation.Erase && true == _isCleanOtherPaintTool)
        {
            _listScribbleTool.Clear();
            for (int i = 0; i < _arrayScribbleTool.Length; i++)
            {
                ScribbleTool tool = _arrayScribbleTool[i];
                for (int j = 0; j < tool.ListPainter.Count; j++)
                {
                    _listPainter.Add(tool.ListPainter[j]);
                }
                _listScribbleTool.Add(tool);
            }
        }

        if (_operation == Operation.Replace && _listPainter.Count == 2)
        {
            RenderTexturePainter mainPainter = _listPainter[0];
            RenderTexturePainter tempPainter = _listPainter[1];
            if (false == mainPainter.IsInited)
            {
                mainPainter.Init();
            }
            tempPainter.SourceTex = mainPainter.SourceTex;
        }
        LoopPainter((RenderTexturePainter painter) =>
        {
            if (false == painter.IsInited)
            {
                painter.Init();
            }
            UpdatePainterSetting(painter);
            if (false == painter._isShowSource)
            {
                painter.ResetCanvas();
            }
            PainterChecker checker = painter.GetComponent<PainterChecker>();
            if (null != checker)
            {
                checker.Init();
                checker.SetDataByTexture(painter.SourceTex.ToTexture2D(), painter._penTex, painter._brushScale, 0);
                _listChecker.Add(checker);
                _dicCheckerIndex.Add(painter, _listChecker.IndexOf(checker));
            }
        });

        PainterChecker[] arrayTempChecker = _arrayChecker;
        if (null != arrayChecker)
        {
            arrayTempChecker = _arrayChecker.Concat(arrayChecker).ToArray();
        }

        for (int i = 0; i < arrayTempChecker.Length; i++)
        {
            PainterChecker checker = arrayTempChecker[i];
            checker.Init();
            _listChecker.Add(checker);
        }
    }

    private void UpdatePainterSetting(RenderTexturePainter painter)
    {
        painter._penTex = null != _penTex ? _penTex : painter._penTex;
        painter._brushScale = _penScale > 0 ? _penScale : painter._brushScale;
    }

    public bool IsPainterInited()
    {
        for (int i = 0; i < _listPainter.Count; i++)
        {
            RenderTexturePainter painter = _listPainter[i];
            if (false == painter.IsInited)
            {
                return false;
            }
        }
        return true;
    }

    public override void OnClick(UGUIDrag drag)
    {

    }

    public void ResetPainter()
    {
        if (_operation == Operation.Replace)
        {
            RenderTexturePainter painter = _listPainter[0];
            painter.IsErase = true;
            painter.ResetCanvas();
            return;
        }
        LoopPainter((RenderTexturePainter painter) =>
        {
            painter.ResetCanvas();
        });
    }

    public void ClearPainter()
    {
        bool isNeedEndDraw = false;
        if (_mode == Mode.MoveFinger && _isFingerMoved)
        {
            _isFingerMoved = false;
            isNeedEndDraw = true;
        }
        LoopPainter((RenderTexturePainter painter) =>
        {
            if (isNeedEndDraw)
            {
                painter.EndDraw();
            }
            painter.ClearCanvas();
        });
        LoopChecker((PainterChecker checker) =>
        {
            if (null != checker)
            {
                checker.Reset();
            }
        });
        _hasSendPainterFinish = false;
        
    }

    public void ShowScribbleComplete()
    {
        if (_operation == Operation.Replace)
        {
            RenderTexturePainter painter = _listPainter[0];
            painter.ShowScribbleComplete();
            return;
        }
        LoopPainter((RenderTexturePainter painter) =>
        {
            painter.ShowScribbleComplete();
        });
    }

    public void ResetPainterStatus(bool isErase)
    {
        LoopPainter((RenderTexturePainter painter) =>
        {
            painter.IsErase = isErase;
        });
    }

    public override void ResetStatus()
    {
        base.ResetStatus();
        if (false == _isKeepStatus && _operation != Operation.Erase)
        {
            FadeOutPaintCanvas();
        }
        UnSelected();
    }

    private void FadeOutPaintCanvas(float time = 1f)
    {
        if (null != _coFadeCanvas)
        {
            StopCoroutine(_coFadeCanvas);
            _coFadeCanvas = null;
        }
        _coFadeCanvas = StartCoroutine(FadeOutPaintCanvasFunc(time));
    }

    private IEnumerator FadeOutPaintCanvasFunc(float time = 1f)
    {
        float perTime = time / 100;
        for (int i = 0; i < _listPainter.Count; i++)
        {
            RenderTexturePainter painter = _listPainter[i];
            if (true == painter.IsInited)
            {
                for (int j = 0; j < 100; i++)
                {
                    painter.CanvasAlpha = 1 - j * 0.01f;
                    yield return new WaitForSeconds(perTime);
                }
                painter.Dispose();
            }
        }
        _coFadeCanvas = null;
    }

    public override void UnSelected()
    {
        base.UnSelected();
        if (_mode == Mode.MoveFinger)
        {
            ResetAnim();
        }
        _isSelected = false;
    }

    public void UpdateTexture(Texture2D texture, int painterIndex = 0, bool isKeepCheckerStatus = false)
    {
        if (_operation == Operation.Replace)
        {
            RenderTexturePainter mainPainter = _listPainter[0];
            RenderTexturePainter tempPainter = _listPainter[1];
            mainPainter.SourceTex = texture;
            tempPainter.SourceTex = texture;
            UpdatePainterSetting(mainPainter);
            UpdatePainterSetting(tempPainter);
            return;
        }
        RenderTexturePainter painter = _listPainter[painterIndex];
        painter.SourceTex = texture;
        UpdatePainterSetting(painter);
        if (isKeepCheckerStatus)
        {
            return;
        }
        painter.ResetCanvas();
        PainterChecker checker = _listChecker[_dicCheckerIndex[painter]];
        if (null != checker && true == checker._isUseChecker)
        {
            checker.Reset();
        }
    }

    private void SetBackupTexture (Texture2D texture)
    {
        if (false == _isUseLastTex)
        {
            return;
        }
        _lastTex = texture;
    }

    public override void OnDragToolBegin(UGUIDrag drag, PointerEventData eventData)
    {
        base.OnDragToolBegin(drag, eventData);
        if (true == _isCleanOtherPaintTool)
        {
            _hasSendPainterFinish = false;
        }
        if (_mode != Mode.DragTool)
        {
            return;
        }
        _isSelected = true;
        StartDragPaint();
    }

    public override void OnDragTool(UGUIDrag drag, PointerEventData eventData)
    {
        base.OnDragTool(drag, eventData);
        if (null == _drag || _mode != Mode.DragTool)
        {
            return;
        }
        if (_operation == Operation.CutShape)
        {
            return;
        }
        if (true == _hasSendPainterFinish)
        {
            return;
        }
        Transform target = null != _drag._triggerPos ? _drag._triggerPos : _drag._tool;
        DoDragPaint(target);
    }

    public override void OnDragToolEnd(UGUIDrag drag, PointerEventData eventData)
    {
        OnInArea.Invoke(false);
        if (_operation == Operation.CutShape)
        {
            OnCutShape.Invoke();
        }
        base.OnDragToolEnd(drag, eventData);
        if (_mode != Mode.DragTool)
        {
            return;
        }
        EndDragPaint();
        _isSelected = false;
    }

    public override void OnDragToolTweenOver(UGUIDrag drag)
    {
        base.OnDragToolTweenOver(drag);
        OnInArea.Invoke(false);
    }

    private void StartDragPaint()
    {
        if (_operation == Operation.Replace || _operation == Operation.ReplaceErase)
        {
            if (true == _isUseLastTex && null != _lastTex)
            {
                UpdateTexture(_lastTex, 0, true);
            }
            RenderTexturePainter mainPainter = _listPainter[0];
            RenderTexturePainter tempPainter = _listPainter[1];
            tempPainter.SourceTex = _operation == Operation.ReplaceErase ? _replaceEraseTex : mainPainter.SourceTex;
            UpdatePainterSetting(mainPainter);
            UpdatePainterSetting(tempPainter);
            mainPainter.IsErase = true;
            tempPainter.CanvasMat.SetFloat("_BlendSrc", (float)BlendMode.SrcAlpha);
            tempPainter.CanvasMat.SetFloat("_BlendDst", (float)BlendMode.OneMinusSrcAlpha);
            if (_operation == Operation.ReplaceErase)
            {
                for (int i = 2; i < _listPainter.Count; i++)
                {
                    RenderTexturePainter painter = _listPainter[i];
                    UpdatePainterSetting(painter);
                    painter.IsErase = false;
                }
            }
        }
        else
        {
            LoopPainter((RenderTexturePainter painter) =>
            {
                UpdatePainterSetting(painter);
                bool isErase = painter.IsErase;
                if (_operation == Operation.Scribble)
                {
                    isErase = false;
                }
                else if (_operation == Operation.Erase || _operation == Operation.CutShape)
                {
                    isErase = true;
                }
                painter.IsErase = isErase;
            });
        }
        _hasDoEndDrawActions = false;
    }

    private void DoDragPaint(Transform target)
    {
        Vector3 toPos = Camera.main.WorldToScreenPoint(target.position);
        if (true == IsPainterInited())
        {
            LoopPainter((RenderTexturePainter painter) =>
            {
                bool temp = painter._useVectorGraphic;
                if (true == _isUsePenTex)
                {
                    painter._useVectorGraphic = false;
                }
                painter.Drawing(toPos);
                if (true == _isUsePenTex)
                {
                    painter._useVectorGraphic = temp;
                }
            });
        }
        UpdateCheckPoint(toPos);
    }

    private void EndDragPaint()
    {
        if (false == _hasDoEndDrawActions)
        {
            DoEndDrawActions();
            _hasDoEndDrawActions = true;
        }
    }

    private void DoEndDrawActions()
    {
        if (_mode == Mode.DragTool)
        {
            if (_operation == Operation.CutShape)
            {
                Transform target = null != _drag._triggerPos ? _drag._triggerPos : _drag._tool;
                Vector3 toPos = Camera.main.WorldToScreenPoint(target.position);
                LoopPainter((RenderTexturePainter painter) =>
                {
                    painter._useVectorGraphic = false;
                    painter.Drawing(toPos, null, true);
                    painter._useVectorGraphic = true;
                });
            }
        }
        DoEndFingerMoveAction();
    }

    private void DoEndFingerMoveAction()
    {
        LoopPainter((RenderTexturePainter painter) =>
        {
            painter.EndDraw();
        });
        if (_operation == Operation.Replace || _operation == Operation.ReplaceErase)
        {
            RenderTexturePainter mainPainter = _listPainter[0];
            RenderTexturePainter tempPainter = _listPainter[1];
            tempPainter.CanvasMat.SetFloat("_BlendSrc", (float)BlendMode.One);
            tempPainter.CanvasMat.SetFloat("_BlendDst", (float)BlendMode.Zero);
            mainPainter.IsErase = false;
            Camera cmain = Camera.main;
            mainPainter.ClickDraw(cmain.WorldToScreenPoint(tempPainter.transform.position), cmain, mainPainter.RednerTex, 1, tempPainter.CanvasMat);
            tempPainter.ClearCanvas();
            if (_operation == Operation.ReplaceErase)
            {
                for (int i = 2; i < _listPainter.Count; i++)
                {
                    RenderTexturePainter painter = _listPainter[i];
                    painter.IsErase = false;
                }
            }
        }
        else if (_operation == Operation.CutShape)
        {
            LoopPainter((RenderTexturePainter painter) =>
            {
                painter.IsErase = !painter.IsErase;
            });
        }
    }

    private void UpdateCheckPoint(Vector3 screenTouchPos)
    {
        if (_operation == Operation.Erase && true == _isCleanOtherPaintTool)
        {
            RemoveCheckPoint(screenTouchPos, _eraseRadius);
        }
        else
        {
            AddCheckPoint(screenTouchPos);
        }
    }

    private void AddCheckPoint(Vector3 screenTouchPos)
    {
        float aOffset = _attachmentRandomOffset;
        float aScale = _attachmentRandomScale;
        bool isInArea = false;
        foreach (PainterChecker checker in _listChecker)
        {
            if (true == checker._isUseChecker)
            {
                Vector3 cPos = checker.CheckPointContainer.InverseTransformPoint(Camera.main.ScreenToWorldPoint(screenTouchPos));
                List<Vector2> checkPoints = checker._checkData.checkPoints;
                for (int i = 0; i < checkPoints.Count; i++)
                {
                    Vector2 v = checkPoints[i];
                    string key = v.x + "-" + v.y;
                    if (Vector2.Distance(cPos, v) < checker._checkData.gridSize.x * _checkDataGridFactor)
                    {
                        isInArea = true;
                        if (false == checker.DicCheckPoint.ContainsKey(key))
                        {
                            if (true == _hasAttachment)
                            {
                                float scale = _prefabAttachment.transform.localScale.x;
                                Transform attachment = Instantiate(_prefabAttachment).transform;
                                attachment.SetParent(checker.CheckPointContainer);
                                attachment.gameObject.name = _prefabAttachment.name;
                                attachment.GetComponent<SpriteRenderer>().enabled = true;
                                attachment.localScale = Vector3.zero;
                                float x = v.x + Random.Range(-aOffset, aOffset);
                                float y = v.y + Random.Range(-aOffset, aOffset);
                                attachment.localPosition = new Vector3(x, y, checker.CheckPointContainer.childCount * 0.01f);
                                attachment.DOScale(scale + Random.Range(-aScale, aScale), 0.3f);
                                _dicAttachment[key] = attachment;
                                if (null != _soundAddAttachment)
                                {
                                    AudioManager.PlaySound(_soundAddAttachment);
                                }
                            }
                            checker.DicCheckPoint[key] = v;
                        }
                    }
                }
            }
        }
        OnInArea.Invoke(isInArea);
        if (true == isInArea)
        {
            if (_isControlDragEffect && _drag._dragEffectType == UGUIDrag.DRAG_EFFECT_TYPE.IN_AREA)
            {
                SetDragEffectPlay(isInArea);
            }
            SetOnAreaOutCoPlay(false);
        }
        else
        {
            SetOnAreaOutCoPlay(true);
        }
        UpdateCheckDataProgress();
    }

    private void SetOnAreaOutCoPlay(bool isPlay)
    {
        if (false == isPlay && null != _coOnAreaOut)
        {
            StopCoroutine(_coOnAreaOut);
            _coOnAreaOut = null;
        }
        else if (true == isPlay && null == _coOnAreaOut)
        {
            _coOnAreaOut = StartCoroutine(AreaOutCoFunc());
        }
    }

    private IEnumerator AreaOutCoFunc()
    {
        yield return new WaitForSeconds(0.6f);
        if (_isControlDragEffect && _drag._dragEffectType == UGUIDrag.DRAG_EFFECT_TYPE.IN_AREA)
        {
            SetDragEffectPlay(false);
        }
        _coOnAreaOut = null;
    }

    private float GetProgress()
    {
        int total = 0;
        int count = 0;
        foreach (PainterChecker checker in _listChecker)
        {
            if (true == checker._isUseChecker && checker._checkData.checkPoints.Count > 0)
            {
                total += checker._checkData.checkPoints.Count;
                count += checker.DicCheckPoint.Keys.Count;
            }
        }
        if (0 == total)
        {
            return 0;
        }
        return Mathf.Clamp01(1f * count / total);
    }

    private float GetPaintPercentage()
    {
        if (Mathf.Abs(0 - _checkerTargetPercentage) < float.Epsilon)
        {
            return 0;
        }
        return GetProgress() / _checkerTargetPercentage;
    }

    private void UpdateCheckDataProgress(float percentage = -1f)
    {
        bool isUseChecker = false;
        foreach (PainterChecker checker in _listChecker)
        {
            if (true == checker._isUseChecker)
            {
                isUseChecker = true;
                break;
            }
        }
        if (false == _isCleanOtherPaintTool && false == isUseChecker)
        {
            return;
        }
        if (Mathf.Abs(-1f - percentage) < float.Epsilon)
        {
            percentage = GetPaintPercentage();
            if (_isCleanOtherPaintTool)
            {
                percentage = (percentage + GetOtherPaintToolPercentage()) / 2f;
            }
            
        }
        bool isFinish = (percentage >= _checkerTargetPercentage);
        percentage = Mathf.Clamp01(percentage);
        _onPainterPercentage.Invoke(percentage);
        if (true == _hasSendPainterFinish)
        {
            return;
        }
        if (percentage > 0.02)
        {
            LoopScribbleTool((ScribbleTool tool) =>
            {
                tool._hasSendPainterFinish = false;
            });
        }
        if (true == isFinish)
        {
            _hasSendPainterFinish = true;
            if (_operation == Operation.Replace)
            {
                _listPainter[0]._isShowSource = true;
            }
            if (false == _hasDoEndDrawActions)
            {
                DoEndDrawActions();
                _hasDoEndDrawActions = true;
            }
            if (_operation == Operation.Scribble || _operation == Operation.Replace)
            {
                ShowScribbleComplete();
            }
            else if (_operation == Operation.Erase)
            {
                ClearPainter();
            }
            OnPainterFinish.Invoke();
        }
    }

    private float GetOtherPaintToolPercentage()
    {
        int total = 0;
        float percentage = 0f;
        LoopScribbleTool((ScribbleTool tool) =>
        {
            float tPercentage = tool.GetProgress();
            if (tPercentage > 0)
            {
                total += 1;
                percentage += (1 - tPercentage);
            }
        });
        if (0 == total)
        {
            return 0;
        }
        percentage = percentage / total;
        return percentage;
    }

    private void RemoveCheckPoint(Vector3 screenTouchPos, float radius, bool isCallFromOtherTool = false)
    {
        bool isInArea = false;
        if (_listChecker.Count > 0)
        {
            foreach (PainterChecker checker in _listChecker)
            {
                if (true == checker._isUseChecker)
                {
                    Vector3 cPos = checker.CheckPointContainer.InverseTransformPoint(Camera.main.ScreenToWorldPoint(screenTouchPos));
                    List<Vector2> checkPoints = checker._checkData.checkPoints;
                    for (int i = 0; i < checkPoints.Count; i++)
                    {
                        Vector2 v = checkPoints[i];
                        string key = v.x + "-" + v.y;
                        if (Vector2.Distance(cPos, v) < radius)
                        {
                            isInArea = true;
                            if (true == checker.DicCheckPoint.ContainsKey(key))
                            {
                                if (true == _hasAttachment)
                                {
                                    RemoveAttachmentAnim(_dicAttachment[key]);
                                    _dicAttachment.Remove(key);
                                }
                                checker.DicCheckPoint.Remove(key);
                            }
                        }
                    }
                }
            }
        }
        if (true == _isCleanOtherPaintTool)
        {
            //bool isCleanDone = true;
            //bool hasAttachment = false;
            LoopScribbleTool((ScribbleTool tool) =>
            {
                int count = tool.DicAttachment.Count;
                tool.RemoveCheckPoint(screenTouchPos, _eraseRadius, true);
                //if (count > tool.DicAttachment.Count)
                //{
                //    hasAttachment = true;
                //}
                //if (tool.DicAttachment.Count > 0)
                //{
                //    isCleanDone = false;
                //}
            });
        }
        if (false == isCallFromOtherTool)
        {
            if (_isControlDragEffect && _drag._dragEffectType == UGUIDrag.DRAG_EFFECT_TYPE.IN_AREA)
            {
                SetDragEffectPlay(isInArea);
            }
            OnInArea.Invoke(isInArea);
            UpdateCheckDataProgress();
        }
    }

    private void RemoveAttachmentAnim(Transform attachment)
    {
        if (null != _soundRemoveAttachment)
        {
            AudioManager.PlaySound(_soundRemoveAttachment);
        }
        attachment.DOScale(0f, 0.25f).OnComplete(() =>
        {
            Destroy(attachment.gameObject);
        });
    }

    public bool HasAttachment()
    {
        return _dicAttachment.Keys.Count > 0;
    }

    public void ClearAttachmentData()
    {
        foreach(Transform attachment in _dicAttachment.Values)
        {
            Destroy(attachment.gameObject);
        }
        _dicAttachment.Clear();
    }

    private void MouseButtonDownAction()
    {

    }

    private void MouseButtonMovingAction()
    {

    }

    private void MouseButtonUpAction()
    {

    }

    void Update()
    {
        if (_mode == Mode.DragTool)
        {
            return;
        }
        if (false == _isSelected)
        {
            return;
        }
        if (true == _isStopFingerTouch)
        {
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (true == _isCheckOnUI && true == InputUtil.CheckMouseOnUI())
            {
                return;
            }
            if (false == _isFingerMoved)
            {
                _hasStartDraw = true;
                _mousePressPos = Input.mousePosition;
                if (_operation == Operation.DrawLine)
                {
                    LoopPainter((RenderTexturePainter painter) =>
                    {
                        painter.ClickDraw(_mousePressPos, null, null, painter._brushScale);
                    });
                }
                else
                {
                    StartDragPaint();
                }
            }
            _hasDoEndDrawActions = false;
            MouseButtonDownAction();
            OnToolStart.Invoke();
            _drag.PlaySoundStartDrag();
        }
        if (Input.GetMouseButton(0) && false == _hasDoEndDrawActions)
        {
            if (false == _isFingerMoved)
            {
                if (Vector3.Distance(Input.mousePosition, _mousePressPos) > 10)
                {
                    _isFingerMoved = true;
                }
            }
            if (null != _finger && true == _isHideFingerOnMove)
            {
                _finger.DOKill();
                _finger.enabled = false;
            }
            if (true == _isFingerMoved)
            {
                bool isUseChecker = false;
                foreach (PainterChecker checker in _listChecker)
                {
                    if (true == checker._isUseChecker)
                    {
                        isUseChecker = true;
                        break;
                    }
                }
                if (false == isUseChecker)
                {
                    OnInArea.Invoke(true);
                    SetDragEffectPlay(true);
                }
            }
            if (true == _isFingerMoved && null != _finger)
            {
                bool ret = RectTransformUtility.ScreenPointToLocalPointInRectangle(_finger.transform.parent.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out Vector2 localPoint);
                if (ret)
                {
                    if (_finger.transform.GetSiblingIndex() == _finger.transform.parent.childCount - 1)
                    {
                        _finger.GetComponent<RectTransform>().anchoredPosition = localPoint;
                        Vector3 toPos = Camera.main.WorldToScreenPoint(_finger.transform.position);
                        LoopPainter((RenderTexturePainter painter) =>
                        {
                            painter.Drawing(toPos);
                        });
                        UpdateCheckPoint(toPos);
                    }
                }
            }
            else
            {
                Vector3 toPos = Input.mousePosition;
                if (_arrayDrawMainTex.Length > 0)
                {
                    Texture tex = _arrayDrawMainTex[Random.Range(0, _arrayDrawMainTex.Length)];
                    toPos.x = toPos.x - 5 + 10 * Random.Range(0f, 1f);
                    toPos.y = toPos.y - 5 + 10 * Random.Range(0f, 1f);
                    float scale = 0.8f + Random.Range(0f, 1f);
                    LoopPainter((RenderTexturePainter painter) =>
                    {
                        painter.ClickDraw(toPos, null, tex, scale);
                    });
                }
                else
                {
                    LoopPainter((RenderTexturePainter painter) =>
                    {
                        painter.Drawing(toPos);
                    });
                }
                UpdateCheckPoint(toPos);
            }
            MouseButtonMovingAction();
        }
        if (Input.GetMouseButtonUp(0))
        {
            OnInArea.Invoke(false);
            SetDragEffectPlay(false);
            if (true == _hasStartDraw)
            {
                _hasStartDraw = false;
                _drag.PlaySoundEndDrag();
                if (true == _isFingerMoved)
                {
                    _isFingerMoved = false;
                    MouseButtonUpAction();
                    OnToolEnd.Invoke();
                }
                EndDragPaint();
            }
        }
    }

}
