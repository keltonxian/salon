using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PureMVC.Core;
using DG.Tweening;
using PureMVC.Const;

public class BeardToolDye : Base
{
    private ScribbleTool _toolScribble = null;
    public ScribbleTool ToolScribble
    {
        get
        {
            return _toolScribble;
        }
    }

    private SpriteHandler _spriteHandler = null;
    public SpriteHandler SpriteHandler
    {
        get
        {
            return _spriteHandler;
        }
    }

    public enum DyeColor
    {
        Red = 1, Green = 2, Blue = 3, Purple = 4,
    }
    public DyeColor _dyeColor = DyeColor.Red;

    private BeardModelHandler _model;

    public void Init(BeardModelHandler model)
    {
        _toolScribble = GetComponent<ScribbleTool>();
        _toolScribble.Init(new RenderTexturePainter[] { model._hairMainPainter, model._hairTempPainter }, null);
        List<SpriteHandler.SpriteData> listSpriteData = new List<SpriteHandler.SpriteData>();
        //string url = "Beard/Dye/" + model.gameObject.name + "/{0}.png";
        //listSpriteData.Add(new SpriteHandler.SpriteData(_toolScribble, _formatUrl, "", SpriteHandler.LoadType.StreamingAsset));
        string url = string.Format("{0}/{1}_{2}_dye.unity3d", AppConst.AppName, AppConst.AppName, model.gameObject.name).ToLower();
        string fileName = "{0}.png";
        listSpriteData.Add(new SpriteHandler.SpriteData(_toolScribble, url, fileName, SpriteHandler.LoadType.AssetBundle));
        _spriteHandler = GetComponent<SpriteHandler>();
        _spriteHandler.Init(listSpriteData);
        _model = model;
        ToolScribble.OnToolStart.AddListener(OnToolStart);
        ToolScribble.OnToolEnd.AddListener(OnToolEnd);
    }

    private void OnToolStart()
    {
        _model.SetShowEyeMove(true);
        UpdatePainter();
    }

    private void OnToolEnd()
    {
        _model.SetShowEyeMove(false);
    }

    private void UpdatePainter()
    {
        _spriteHandler.SelectItem((int)_dyeColor);
    }
}
