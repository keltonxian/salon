using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BeardToolRazor : MonoBehaviour
{
    public Transform _blade = null;
    private Vector3 _markRotate;

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
    public BeardFaceRed _faceRed = null;
    public BeardFaceScar _faceScar = null;
    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _markRotate = _blade.localEulerAngles;
        _toolScribble = GetComponent<ScribbleTool>();
        _toolScribble.Init(new RenderTexturePainter[] { model._beardFrontPainter, model._beardMiddlePainter, model._beardBackPainter }, null);
        _toolCollider = GetComponent<ColliderTool>();
        _toolCollider.Init(new GameObject[] { model._beardMessy, model._beardRed, model._beardScar });
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
        Collider2D c = ToolCollider.CurrentCollider;
        if (c.transform.parent.parent.parent.transform == _model._beardMessy.transform)
        {
            _faceHair.TriggerMessy(c, true);
        }
        else if (c.transform.parent.transform == _model._beardRed.transform)
        {
            _faceRed.TriggerRed(c, true);
        }
        else if (c.transform.parent.transform == _model._beardScar.transform)
        {
            _faceScar.TriggerScar(c, true);
        }
    }

    public void SetUse(bool isUse)
    {
        _blade.DOKill();
        _blade.DORotate(isUse ? new Vector3(0, 0, -120f) : _markRotate, 0.15f, RotateMode.Fast);
    }

}
