using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeardToolBubble : MonoBehaviour
{
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
        _toolScribble = GetComponent<ScribbleTool>();
        _toolScribble.Init(null, new PainterChecker[] { model._beardBubbleChecker });
        _model = model;
    }
}
