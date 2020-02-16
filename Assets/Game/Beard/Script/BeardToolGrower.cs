using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeardToolGrower : MonoBehaviour
{
    public BeardFaceHair _faceHair;
    public BeardFaceFur _faceFur;
    public BeardFaceRed _faceRed;
    public BeardFaceScar _faceScar;

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
        _toolCollider.Init(new GameObject[] { model._beardMessy, model._beardFur, model._beardRed, model._beardScar });
        _toolScribble = GetComponent<ScribbleTool>();
        _toolScribble.Init(new RenderTexturePainter[] { model._beardBackPainter, model._beardMiddlePainter, model._beardFrontPainter }, null);
        ToolCollider.OnToolStart.AddListener(OnToolStart);
        ToolCollider.OnToolEnd.AddListener(OnToolEnd);
        _model = model;
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
        Collider2D c = ToolCollider.CurrentCollider;
        if (c.transform.parent.parent.parent.transform == _model._beardMessy.transform)
        {
            _faceHair.TriggerMessy(_toolCollider.CurrentCollider, false);
        }
        else if (c.transform.parent.transform == _model._beardFur.transform)
        {
            _faceFur.TriggerFur(c, false);
        }
        else if (c.transform.parent.transform == _model._beardRed.transform)
        {
            _faceRed.TriggerRed(c, false);
        }
        else if (c.transform.parent.transform == _model._beardScar.transform)
        {
            _faceScar.TriggerScar(c, false);
        }
    }

}
