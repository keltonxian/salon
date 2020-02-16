using System.Collections;
using System.Collections.Generic;
using PureMVC.Core;
using UnityEngine;

public class CaptureKit : Base
{
    public Camera _camera;
    public Transform _targetParent;
    public RectTransform _leftTop;
    public RectTransform _rightBottom;

    private int _width = 0;
    private int _height = 0;

    //void Awake()
    //{
    //    Init();
    //}

    public void Init()
    {
        _width = Mathf.CeilToInt(_rightBottom.anchoredPosition.x - _leftTop.anchoredPosition.x);
        _height = Mathf.CeilToInt(_leftTop.anchoredPosition.y - _rightBottom.anchoredPosition.y);
    }

    private RenderTexture Capture()
    {
        RenderTexture renderTexture = RenderTexture.GetTemporary(_width, _height, 32, RenderTextureFormat.ARGB32);
        renderTexture.autoGenerateMips = false;
        Graphics.SetRenderTarget(renderTexture);
        GL.Clear(true, true, Color.clear);
        RenderTexture markRenderTexture = _camera.targetTexture;
        _camera.targetTexture = renderTexture;
        _camera.Render();
        _camera.targetTexture = markRenderTexture;
        return renderTexture;
    }

    private Texture2D CaptureTexture2D(GameObject obj, GameObject leftTop = null, GameObject rightBottom = null)
    {
        GL.Clear(true, true, Color.clear);
        Transform target = obj.transform;
        Transform parent = target.parent;
        int targetIndex = -1;
        for (int i = 0; i < parent.childCount - 1; i++)
        {
            if (target == parent.GetChild(i).transform)
            {
                targetIndex = i;
                break;
            }
        }
        Vector3 targetLocalPosition = target.localPosition;
        target.SetParent(_targetParent);
        Vector3 markPos = target.localPosition;
        markPos.z = targetLocalPosition.z;
        target.localPosition = markPos;
        RenderTexture renderTexture = Capture();
        int width = renderTexture.width;
        int height = renderTexture.height;
        Texture2D texture2d = null;
        if (null != leftTop && null != rightBottom)
        {
            Vector2 lt1 = leftTop.GetComponent<RectTransform>().anchoredPosition;
            Vector2 rb1 = rightBottom.GetComponent<RectTransform>().anchoredPosition;
            Rect rect = new Rect();
            rect.x = _width / 2 + lt1.x;
            rect.y = _height / 2 + rb1.y;
            rect.width = Mathf.CeilToInt(rb1.x - lt1.x);
            rect.height = Mathf.CeilToInt(lt1.y - rb1.y);
            texture2d = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            texture2d.ReadPixels(rect, 0, 0, false);
            texture2d.Apply();
        }
        else
        {
            texture2d = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            texture2d.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2d.Apply();
        }
        target.SetParent(parent);
        target.localPosition = targetLocalPosition;
        if (-1 != targetIndex)
        {
            target.SetSiblingIndex(targetIndex);
        }
        return texture2d;
    }

    public Sprite CaptureSprite(GameObject obj, GameObject leftTop = null, GameObject rightBottom = null)
    {
        Texture2D texture2d = CaptureTexture2D(obj, leftTop, rightBottom);
        Sprite sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), Vector2.one * 0.5f);
        return sprite;
    }
}
