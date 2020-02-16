using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainterChecker : MonoBehaviour
{
    public bool _isUseChecker = true;
#if UNITY_EDITOR
    public bool _gridDefaultStatus = false;
    public Texture _sourceTex;
    public Texture _penTex;

    public Color _enableColor = new Color(0f, 0f, 1f, 0.4f);
    public Color _disableColor = new Color(1f, 0.92f, 0.016f, 0.1f);

    public Dictionary<string, Rect> _dicGrid;
    public Dictionary<string, bool> _dicEnable;
#endif

    [Header("Check Data File")]
    public PaintCheckData _checkData;
    public bool _canResetData = false;

    public Transform CheckPointContainer
    {
        get
        {
            return transform;
        }
    }

    private RenderTexturePainter _painter;
    public RenderTexturePainter Painter
    {
        get
        {
            return _painter;
        }
    }
    private Dictionary<string, Vector2> _dicCheckPoint = new Dictionary<string, Vector2>();
    public Dictionary<string, Vector2> DicCheckPoint
    {
        get
        {
            return _dicCheckPoint;
        }
    }
    [Range(0, 1)]
    public float _targetPercentage = 0f;

#if UNITY_EDITOR
    [Range(0.1f, 1f)]
    public float _gridScale = 1f;
    [Range(0.01f, 1f)]
    public float _editorBrushSize = 0.2f;
#endif

    private bool _isDown;
    private Vector3 _prevMousePosition;
    private Vector2 _lerpSize;
    private int _totalCount;
    private List<Vector2> _checkPoints;
    private List<Vector2> _removePoints;

    public float Progress
    {
        get
        {
            return 1f - (float)_checkPoints.Count / _totalCount;
        }
    }

    public void Init()
    {
        _painter = GetComponent<RenderTexturePainter>();
#if UNITY_EDITOR
        _canResetData = true;
#endif
        Reset();
    }

    public void Reset()
    {
        if (null != _checkData)
        {
            int count = _checkData.checkPoints.Count;
            _totalCount = count;

            if (_canResetData)
            {
                _checkPoints = new List<Vector2>();
            }
            else
            {
                _checkPoints = _checkData.checkPoints;
            }
            _removePoints = new List<Vector2>();

#if UNITY_EDITOR
            _dicGrid = new Dictionary<string, Rect>();
            _dicEnable = new Dictionary<string, bool>();
#endif

            for (int i = 0; i < count; i++)
            {
                Vector2 v = _checkData.checkPoints[i];
                if (_canResetData)
                {
                    _checkPoints.Add(v);
                }

#if UNITY_EDITOR
                Rect rect = new Rect(v.x - _checkData.gridSize.x * 0.005f, v.y - _checkData.gridSize.y * 0.005f, _checkData.gridSize.x * 0.01f, _checkData.gridSize.y * 0.01f);
                string key = v.x + "-" + v.y;
                _dicGrid[key] = rect;
                _dicEnable[key] = true;
#endif
            }
        }
        if (_painter && _painter._penTex)
        {
            float w = _painter._penTex.width * _painter._brushScale * 0.005f;
            float h = _painter._penTex.height * _painter._brushScale * 0.005f;
            _lerpSize = new Vector2(w, h);
        }
        _dicCheckPoint.Clear();
    }

    public void ClickDraw(Vector3 screenPos, Camera camera = null, bool isReverse = false)
    {
        if (null == camera)
        {
            camera = Camera.main;
        }
        Vector3 localPos = transform.InverseTransformPoint(camera.ScreenToWorldPoint(screenPos));

        float w = _lerpSize.x;
        float h = _lerpSize.y;
        float lerpDamp = Mathf.Min(w, h);
        Rect brushSize = new Rect((localPos.x - w * 0.5f), (localPos.y - h * 0.5f), w, h);

        if (isReverse)
        {
            for (int i = 0; i < _removePoints.Count; i++)
            {
                Vector2 point = _removePoints[i];
                if (Vector2.Distance(point, brushSize.center) < lerpDamp * 0.75f)
                {
                    _checkPoints.Add(point);
                    _removePoints.RemoveAt(i);
                    --i;

#if UNITY_EDITOR
                    string key = point.x + "-" + point.y;
                    Rect rect = new Rect(point.x - _checkData.gridSize.x * 0.005f, point.y - _checkData.gridSize.y * 0.005f, _checkData.gridSize.x * 0.01f, _checkData.gridSize.y * 0.01f);
                    _dicGrid[key] = rect;
                    _dicEnable[key] = true;
#endif
                }
            }
        }
        else
        {
            for (int i = 0; i < _checkPoints.Count; i++)
            {
                Vector2 point = _checkPoints[i];
                if (Vector2.Distance(point, brushSize.center) < lerpDamp * 0.75f)
                {
                    _removePoints.Add(point);
                    _checkPoints.RemoveAt(i);
                    --i;

#if UNITY_EDITOR
                    string key = point.x + "-" + point.y;
                    _dicGrid.Remove(key);
                    _dicEnable.Remove(key);
#endif
                }
            }
        }
    }

    public bool CheckPosValid(Vector3 screenPos, Camera camera = null, bool isReverse = false)
    {
        if (null == camera)
        {
            camera = Camera.main;
        }
        Vector3 localPos = transform.InverseTransformPoint(camera.ScreenToWorldPoint(screenPos));

        float w = _lerpSize.x;
        float h = _lerpSize.y;
        float lerpDamp = Mathf.Min(w, h);
        Rect brushSize = new Rect((localPos.x - w * 0.5f), (localPos.y - h * 0.5f), w, h);

        if (isReverse)
        {
            for (int i = 0; i < _removePoints.Count; ++i)
            {
                Vector2 point = _removePoints[i];
                if (Vector2.Distance(point, brushSize.center) < lerpDamp * 0.75f)
                {
                    return true;
                }
            }
        }
        else
        {
            for (int i = 0; i < _checkPoints.Count; ++i)
            {
                Vector2 point = _checkPoints[i];
                if (Vector2.Distance(point, brushSize.center) < lerpDamp * 0.75f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Drawing(Vector3 screenPos, Camera camera = null)
    {
        if (null == camera)
        {
            camera = Camera.main;
        }
        Vector3 localPos = transform.InverseTransformPoint(camera.ScreenToWorldPoint(screenPos));

        if (!_isDown)
        {
            _isDown = true;
            _prevMousePosition = localPos;
        }

        if (_isDown)
        {
            LerpDraw(localPos, _prevMousePosition);
            _prevMousePosition = localPos;
        }
    }

    protected void LerpDraw(Vector3 current, Vector3 previous)
    {
        float distance = Vector2.Distance(current, previous);
        if (distance > 0f)
        {
            float w = _lerpSize.x;
            float h = _lerpSize.y;

            float lerpDamp = Mathf.Min(w, h);
            Vector2 pos;
            for (float i = 0; i < distance; i += lerpDamp)
            {
                float lDelta = i / distance;
                float lDifx = current.x - previous.x;
                float lDify = current.y - previous.y;
                pos.x = previous.x + (lDifx * lDelta);
                pos.y = previous.y + (lDify * lDelta);

                Rect brushSize = new Rect((pos.x - w * 0.5f), (pos.y - h * 0.5f), w, h);
                for (int j = 0; j < _checkPoints.Count; ++j)
                {
                    Vector2 point = _checkPoints[j];
                    if (Vector2.Distance(point, brushSize.center) < lerpDamp * 0.75f)
                    {
                        _checkPoints.RemoveAt(j);
                        --j;

#if UNITY_EDITOR
                        string key = point.x + "-" + point.y;
                        _dicGrid.Remove(key);
                        _dicEnable.Remove(key);
#endif
                    }
                }
            }
        }
    }

    public void EndDraw()
    {
        _isDown = false;
    }

    public void SetDataByTexture(Texture2D texture, Texture penTexure, float brushScale, byte minAlpha = 10)
    {
        int gx = (int)(penTexure.width * brushScale / 4);
        int gy = (int)(penTexure.height * brushScale / 4);

        _checkData = ScriptableObject.CreateInstance<PaintCheckData>();
        _checkData.checkPoints = new List<Vector2>();
        _checkData.gridSize = new Vector2(gx, gy);

        byte[] bytes = texture.GetRawTextureData();
        int len = bytes.Length;
        int row = 0, col = 0;
        int pixelCount = 0;
        for (int i = 0; i < len; i += 4)
        {
            byte a = bytes[i + 3];
            if (texture.format == TextureFormat.ARGB32)
            {
                a = bytes[i];
            }
            if (a > minAlpha && col % gx == 0 && row % gy == 0)
            {
                _checkData.checkPoints.Add(new Vector2(col - texture.width / 2, row - texture.height / 2) * 0.01f);
            }

            ++col;
            ++pixelCount;
            if (pixelCount >= texture.width)
            {
                pixelCount = 0;
                ++row;
                col = 0;
            }
        }
        Reset();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (_dicGrid != null && _dicEnable != null)
        {

            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            foreach (string key in _dicGrid.Keys)
            {
                Rect rect = _dicGrid[key];
                if (_dicEnable[key])
                {
                    Gizmos.color = _enableColor;
                }
                else
                {
                    Gizmos.color = _disableColor;
                }
                Vector3 center = new Vector3(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
                Vector3 size = new Vector3(rect.width, rect.height, 0.1f);

                Gizmos.DrawWireCube(center, size);
            }

            Gizmos.matrix = oldGizmosMatrix;
        }
    }
#endif

}
