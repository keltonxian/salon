using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeardToolShaver : MonoBehaviour
{
    public enum Shape
    {
        None, Round, Sharp,
    }
    public RectTransform _shaver = null;
    public Shape _shape = Shape.None;
    private Sequence _animShock = null;

    private ScribbleTool _toolScribble = null;
    public ScribbleTool ToolScribble
    {
        get
        {
            return _toolScribble;
        }
    }
    private ColliderTool _toolCollider = null;
    public ColliderTool ToolCollider
    {
        get
        {
            return _toolCollider;
        }
    }

    public BeardFaceHair _faceHair = null;
    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _toolScribble = GetComponent<ScribbleTool>();
        RenderTexturePainter[] arrayPainter = null;
        if (_shape == Shape.Round)
        {
            arrayPainter = new RenderTexturePainter[] { model._beardFrontPainter, model._beardMiddlePainter };
        }
        else if (_shape == Shape.Sharp)
        {
            arrayPainter = new RenderTexturePainter[] { model._beardFrontPainter };
        }
        _toolScribble.Init(arrayPainter, null);
        _toolCollider = GetComponent<ColliderTool>();
        _toolCollider.Init(new GameObject[] { model._beardMessy });
        _model = model;
        ToolScribble.OnToolStart.AddListener(OnToolStart);
        ToolScribble.OnToolEnd.AddListener(OnToolEnd);
    }

    private void OnToolStart()
    {
        _model.SetShowEyeMove(true);
    }

    private void OnToolEnd()
    {
        _model.SetShowEyeMove(false);
    }

    public void OnCollisionHover()
    {
        _faceHair.TriggerMessy(ToolCollider.CurrentCollider, true);
    }

    public void SetShock(bool isShock)
    {
        if (false == isShock)
        {
            _animShock.Kill();
            _animShock = null;
            return;
        }
        if (null != _animShock)
        {
            return;
        }
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_shaver.DOAnchorPosX(5f, 0.05f).From());
        sequence.Append(_shaver.DOAnchorPosY(-5f, 0.05f).From());
        sequence.Append(_shaver.DOAnchorPosY(5f, 0.05f).From());
        sequence.Append(_shaver.DOAnchorPosX(-5f, 0.05f).From());
        //sequence.OnComplete(() => {
        //});
        sequence.SetLoops(-1, LoopType.Yoyo);
        _animShock = sequence;
    }

    public void ShowDone()
    {
        ToolScribble.ShowScribbleComplete();
    }

}
