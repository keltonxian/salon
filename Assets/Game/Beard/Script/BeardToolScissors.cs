using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PureMVC.Core;

public class BeardToolScissors : Base
{
    public Transform _scissorLeft = null;
    private Vector3 _markScissorLeftRotate;
    public Transform _scissorRight = null;
    private Vector3 _markScissorRightRotate;

    private bool _isAnimating = false;
    public AudioClip _soundTrigger;

    public BeardFaceHair _faceHair = null;

    private ColliderTool _toolCollider = null;
    public ColliderTool ToolCollider
    {
        get
        {
            return _toolCollider;
        }
    }

    private ScribbleTool _toolScribble = null;
    public ScribbleTool ToolScribble
    {
        get
        {
            return _toolScribble;
        }
    }

    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _toolCollider = GetComponent<ColliderTool>();
        _toolCollider.Init(new GameObject[] { model._beardMessy });
        _toolScribble = GetComponent<ScribbleTool>();
        _toolScribble.Init();
        _markScissorLeftRotate = _scissorLeft.localEulerAngles;
        _markScissorRightRotate = _scissorRight.localEulerAngles;
        _model = model;
        ToolCollider.OnToolStart.AddListener(OnToolStart);
        ToolCollider.OnToolEnd.AddListener(OnToolEnd);
    }

    private void OnToolStart()
    {
        _model.SetShowHappy(true);
    }

    private void OnToolEnd()
    {
        _model.SetShowHappy(false);
    }

    public void OnCollisionHover()
    {
        TriggerAnim(ToolCollider.CurrentCollider);
    }

    private void TriggerAnim(Collider2D collider2d)
    {
        if (true == _isAnimating)
        {
            return;
        }
        _isAnimating = true;
        if (null != _soundTrigger)
        {
            AudioManager.PlaySound(_soundTrigger);
        }
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_scissorLeft.DORotate(new Vector3(0, 0, -24f), 0.1f, RotateMode.Fast));
        sequence.Join(_scissorRight.DORotate(new Vector3(0, 0, 24f), 0.1f, RotateMode.Fast));
        sequence.Append(_scissorLeft.DORotate(new Vector3(0, 0, 7f), 0.1f, RotateMode.Fast));
        sequence.Join(_scissorRight.DORotate(new Vector3(0, 0, -7f), 0.1f, RotateMode.Fast));
        sequence.AppendCallback(() =>
        {
            _faceHair.TriggerMessy(collider2d, true);
        });
        sequence.Append(_scissorLeft.DORotate(_markScissorLeftRotate, 0.1f, RotateMode.Fast));
        sequence.Join(_scissorRight.DORotate(_markScissorRightRotate, 0.1f, RotateMode.Fast));
        sequence.AppendCallback(() =>
        {
            _isAnimating = false;
        });
    }

    public void ShowDone()
    {
        List<Collider2D> list = ToolCollider.ListCollider;
        foreach(Collider2D c in list)
        {
            _faceHair.TriggerMessy(c, true);
        }
    }

}
