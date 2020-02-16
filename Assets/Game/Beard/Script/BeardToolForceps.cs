using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PureMVC.Core;
using DG.Tweening;

public class BeardToolForceps : Base
{
    public Image _state1;
    public Image _state2;
    public AudioClip _soundTrigger;
    private bool _isAnimating = false;
    public BeardFaceFur _faceFur;

    private ColliderTool _toolCollider = null;
    public ColliderTool ToolCollider
    {
        get
        {
            return _toolCollider;
        }
    }

    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _state1.enabled = true;
        _state2.enabled = false;
        _toolCollider = GetComponent<ColliderTool>();
        _toolCollider.Init(new GameObject[] { model._beardFur });
        _model = model;
        ToolCollider.OnToolStart.AddListener(OnToolStart);
        ToolCollider.OnToolEnd.AddListener(OnToolEnd);
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
        _state1.enabled = false;
        _state2.enabled = true;
        _model.InsertSad();
        _faceFur.TriggerFur(collider2d, true);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.5f);
        sequence.AppendCallback(() => {
            _state1.enabled = true;
            _state2.enabled = false;
            _isAnimating = false;
        });
    }
}
