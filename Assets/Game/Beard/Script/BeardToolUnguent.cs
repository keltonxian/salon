using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeardToolUnguent : MonoBehaviour
{
    private ColliderTool _toolCollider = null;
    public ColliderTool ToolCollider
    {
        get
        {
            return _toolCollider;
        }
    }

    public BeardFaceRed _faceRed = null;
    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _toolCollider = GetComponent<ColliderTool>();
        _toolCollider.Init(new GameObject[] { model._beardRed });
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
        _faceRed.TriggerRed(ToolCollider.CurrentCollider, false, ToolCollider);
    }
}
