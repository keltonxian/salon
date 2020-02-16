using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RenderTexturePainter : MonoBehaviour
{
    #region enums
    public enum PaintType
    {
        None,
        Scribble,
        DrawLine,
        DrawColorfulLine,
    }
    #endregion

#if UNITY_EDITOR
    public Color _gizmosColor = Color.red;
#endif

    [SerializeField]
    protected string _sortingLayerName = "Default";
    public string SortingLayerName
    {
        get
        {
            return _sortingLayerName;
        }
        set
        {
            _sortingLayerName = value;
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                meshRenderer.sortingLayerName = value;
            }
        }
    }

    [SerializeField]
    protected int _sortingOrder = 0;
    public int SortingOrder
    {
        get
        {
            return _sortingOrder;
        }
        set
        {
            _sortingOrder = value;
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                meshRenderer.sortingOrder = value;
            }
        }
    }

    [Header("Paint Canvas Setting")]
    public bool _useSourceTexSize = true;
    [SerializeField]
    private int _canvasWidth = 512;
    [SerializeField]
    private int _canvasHeight = 512;
    [SerializeField]
    private Color _canvasColor = new Color(1, 1, 1, 0);

    [Header("Painter Setting")]
    public PaintType _paintType = PaintType.Scribble;
    public bool _useVectorGraphic = true;
    public Texture _penTex;
    [SerializeField]
    private Texture _sourceTex;
    [SerializeField]
    private Texture _maskTex;

    [SerializeField]
    private Shader _paintShader;
    [SerializeField]
    private Shader _scribbleShader;

    [SerializeField]
    private UnityEngine.Rendering.CullMode _cullMode = UnityEngine.Rendering.CullMode.Back;

    [SerializeField]
    private Color _penColor = new Color(1, 0, 0, 1);

    [Range(0.1f, 5f)]
    public float _brushScale = 1f;

    [Range(0.01f, 2f)]
    public float _drawLerpDamp = 0.02f;

    [SerializeField]
    private bool _isErase = false;

    [Header("Colorful Paint Setting")]
    public Color[] _paintColorful;
    [Range(0f, 10f)]
    public float _colorChangeRate = 1f;

    [Header("Auto Setting")]
    public bool _isAutoInit = false;
    public bool _isAutoDestroy = true;
    public bool _isShowSource = false;

    [SerializeField]
    private RenderTexture _renderTex;
    [SerializeField]
    private bool _isInited = false;
    [SerializeField]
    private Material _penMat;
    [SerializeField]
    private Material _canvasMat;

    private int _colorfulIndex = 1;
    private Vector3 _prevMousePosition;
    private float _colorfulTime = 0f;
    private bool _isDrawing = false;
    private Rect _uv = new Rect(0f, 0f, 1f, 1f);

    #region getter / setter
    public RenderTexture RednerTex
    {
        get
        {
            return _renderTex;
        }
    }

    public bool IsInited
    {
        get
        {
            return _isInited;
        }
    }

    public Material PenMat
    {
        get
        {
            return _penMat;
        }
    }

    public Material CanvasMat
    {
        get
        {
            return _canvasMat;
        }
    }

    public bool IsErase
    {
        get
        {
            return _isErase;
        }
        set
        {
            if (_isErase != value)
            {
                _isErase = value;
                if (IsInited)
                {
                    _penMat.SetFloat("_Cutoff", 0f);
                    if (IsErase)
                    {
                        _penMat.SetFloat("_BlendSrc", (int)BlendMode.Zero);
                        _penMat.SetFloat("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                        if(_paintType==PaintType.DrawLine || _paintType == PaintType.DrawColorfulLine)
                        {
                            _penMat.SetFloat("_FactorA", (int)BlendMode.Zero);
                        }
                    }
                    else
                    {
                        _penMat.SetFloat("_BlendSrc", (int)BlendMode.SrcAlpha);
                        if (_paintType == PaintType.DrawLine || _paintType == PaintType.DrawColorfulLine)
                        {
                            _penMat.SetFloat("_BlendDst", (int)BlendMode.OneMinusSrcAlpha);
                            _penMat.SetFloat("_FactorA", (int)BlendMode.One);
                        }
                        else if (_paintType == PaintType.None)
                        {
                            _penMat.SetFloat("_BlendDst", (int)BlendMode.SrcAlpha);
                        }
                        else
                        {
                            _penMat.SetFloat("_BlendDst", (int)BlendMode.One);
                        }
                    }
                }
            }
        }
    }

    public Color PenColor
    {
        get
        {
            return _penColor;
        }
        set
        {
            _penColor = value;
        }
    }

    public Color CanvasColor
    {
        get
        {
            return _canvasColor;
        }
        set
        {
            _canvasColor = value;
            if (null != _canvasMat)
            {
                _canvasMat.color = CanvasColor;
            }
        }
    }

    public float CanvasAlpha
    {
        get
        {
            return _canvasColor.a;
        }
        set
        {
            _canvasColor.a = value;
            if (null != _canvasMat)
            {
                _canvasMat.SetFloat("_Alpha", value);
            }
        }
    }

    public Texture SourceTex
    {
        get
        {
            return _sourceTex;
        }
        set
        {
            _sourceTex = value;
            if (null != _canvasMat)
            {
                _canvasMat.SetTexture("_SourceTex", value);
            }
        }
    }

    public Texture MaskTex
    {
        get
        {
            return _maskTex;
        }
        set
        {
            _maskTex = value;
            if (null != _canvasMat)
            {
                _canvasMat.SetTexture("_MaskTex", value);
            }
        }
    }

    public int CanvasWidth
    {
        get
        {
            return _canvasWidth;
        }
        set
        {
            if (value > 1)
            {
                _canvasWidth = value;
                if (null != _renderTex)
                {
                    _renderTex.width = _canvasWidth;
                }
            }
        }
    }

    public int CanvasHeight
    {
        get
        {
            return _canvasHeight;
        }
        set
        {
            if (value > 1)
            {
                _canvasHeight = value;
                if (null != _renderTex)
                {
                    _renderTex.height = _canvasHeight;
                }
            }
        }
    }

    public Shader PaintShader
    {
        get
        {
            return _paintShader;
        }
        set
        {
            _paintShader = value;
        }
    }

    public Shader ScribbleShader
    {
        get
        {
            return _scribbleShader;
        }
        set
        {
            _scribbleShader = value;
        }
    }
    #endregion

    void Start()
    {
        if (_isAutoInit)
        {
            Init();
        }
    }

    void OnDestroy()
    {
        if (_isAutoDestroy)
        {
            Dispose();
        }
    }

    public void Init()
    {
        if (!_isInited)
        {
            _isInited = true;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (null != sr)
            {
                if (null == _sourceTex)
                {
                    Sprite sprite = sr.sprite;
                    _sourceTex = sprite.texture;
                }
                DestroyImmediate(sr, false);
            }

            if (_useSourceTexSize && _sourceTex)
            {
                _canvasWidth = _sourceTex.width;
                _canvasHeight = _sourceTex.height;
            }

            _renderTex = new RenderTexture(_canvasWidth, _canvasHeight, 0, RenderTextureFormat.ARGB32);
            _renderTex.filterMode = FilterMode.Trilinear;
            _renderTex.useMipMap = false;

            if (_paintType == PaintType.Scribble)
            {
                _canvasMat = CreateMat(_scribbleShader, _canvasColor, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, 1, 0.02f);
                CreateQuad(_canvasMat);
                _canvasMat.SetTexture("_SourceTex", _sourceTex);
                _canvasMat.SetTexture("_RenderTex", _renderTex);
            }
            else if (_paintType == PaintType.DrawLine || _paintType == PaintType.DrawColorfulLine)
            {
                _canvasColor = Color.white;
                _canvasMat = CreateMat(_paintShader, _canvasColor, BlendMode.One, BlendMode.OneMinusSrcAlpha, 1, 0.0f);
                CreateQuad(_canvasMat);
                _canvasMat.SetTexture("_MainTex", _renderTex);
            }
            else
            {
                _canvasMat = CreateMat(_paintShader, _canvasColor, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, 1, 0.02f);
                CreateQuad(_canvasMat);
                _canvasMat.SetTexture("_MainTex", _renderTex);
            }

            if (null != _maskTex)
            {
                _canvasMat.SetTexture("_MaskTex", _maskTex);
            }

            if (true == _isErase)
            {
                _canvasColor.a = 1f;
                _penMat = CreateMat(_paintShader, _penColor, BlendMode.Zero, BlendMode.OneMinusSrcAlpha, 1, 0.01f);
            }
            else
            {
                if (_paintType == PaintType.Scribble)
                {
                    _canvasColor.a = 0f;
                    _penMat = CreateMat(_paintShader, _penColor, BlendMode.SrcAlpha, BlendMode.One, 1, 0.01f);
                }
                else if (_paintType == PaintType.DrawLine || _paintType == PaintType.DrawColorfulLine)
                {
                    _penMat = CreateMat(_paintShader, _penColor, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, _penColor.a, 0.0f);
                }
                else if (_paintType == PaintType.None)
                {
                    _penMat = CreateMat(_paintShader, _penColor, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, 1, 0.01f);
                }
            }

            if (_isShowSource)
            {
                if(null != _renderTex && null != _sourceTex)
                {
                    Graphics.SetRenderTarget(_renderTex);
                    Graphics.Blit(_sourceTex, _renderTex);
                    RenderTexture.active = null;
                }
            }
            else
            {
                ResetCanvas();
            }
        }
    }

    public Material CreateMat (Shader shader, Color color, BlendMode bmSrc, BlendMode bmDst, float alpha = 1f, float cutoff = 0f)
    {
        Material mat = new Material(shader);
        mat.SetFloat("_BlendSrc", (int)bmSrc);
        mat.SetFloat("_BlendDst", (int)bmDst);
        mat.SetColor("_Color", color);
        mat.SetFloat("_Cutoff", cutoff);
        mat.SetFloat("_Alpha", alpha);
        mat.SetFloat("_CullMode", (int)_cullMode);
        return mat;
    }

    protected void CreateQuad(Material mat)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(_canvasWidth * 0.005f, _canvasHeight * 0.005f),
            new Vector3(_canvasWidth * 0.005f, -_canvasHeight * 0.005f),
            new Vector3(-_canvasWidth * 0.005f, -_canvasHeight * 0.005f),
            new Vector3(-_canvasWidth * 0.005f, _canvasHeight * 0.005f)
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(1,1),
            new Vector2(1,0),
            new Vector2(0,0),
            new Vector2(0,1)
        };
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (null == meshFilter)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (null == meshRenderer)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshRenderer.material = mat;
        meshRenderer.sortingLayerName = SortingLayerName;
        meshRenderer.sortingOrder = SortingOrder;
    }

    public void ResetCanvas()
    {
        if (null == _renderTex)
        {
            return;
        }
        Graphics.SetRenderTarget(_renderTex);
        Color color = _canvasColor;
        if (_isErase)
        {
            color.a = 1f;
            GL.Clear(true, true, color);
        }
        else
        {
            color.a = 0f;
            if (_paintType == PaintType.DrawLine || _paintType == PaintType.DrawColorfulLine)
            {
                color = new Color(0, 0, 0, 0);
            }
            GL.Clear(true, true, color);
        }
        RenderTexture.active = null;
    }

    public void ClickDraw (Vector3 screenPos, Camera camera = null, Texture pen = null, float penScale = 1f, Material drawMat = null, RenderTexture renderTexture = null)
    {
        if (null == camera)
        {
            camera = Camera.main;
        }
        if (null == pen)
        {
            pen = _penTex;
        }
        if (null == drawMat)
        {
            drawMat = _penMat;
        }
        if (null == renderTexture)
        {
            renderTexture = _renderTex;
        }
        Vector3 uvPos = SpriteHitPoint2UV(camera.ScreenToWorldPoint(screenPos));
        if (_uv.Contains(uvPos))
        {
            screenPos = new Vector3(uvPos.x * _canvasWidth, _canvasHeight - uvPos.y * _canvasHeight, 0f);
            float w = pen.width * penScale;
            float h = pen.height * penScale;
            Rect rect = new Rect((screenPos.x - w * 0.5f), (screenPos.y - h * 0.5f), w, h);
            _uv.width = _canvasWidth;
            _uv.height = _canvasHeight;
            if (Intersect(ref rect, ref _uv))
            {
                _penMat.color = _penColor;
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, _canvasWidth, _canvasHeight, 0);
                RenderTexture.active = renderTexture;
                Graphics.DrawTexture(rect, pen, drawMat);
                RenderTexture.active = null;
                GL.PopMatrix();
            }
        }
    }

    protected Vector2 SpriteHitPoint2UV (Vector3 hitPoint)
    {
        Vector3 localPos = transform.InverseTransformPoint(hitPoint);
        localPos *= 100f;
        localPos.x += _canvasWidth * 0.5f;
        localPos.y += _canvasHeight * 0.5f;
        return new Vector2(localPos.x / _canvasWidth, localPos.y / _canvasHeight);
    }

    protected bool Intersect (ref Rect a, ref Rect b)
    {
        bool c1 = a.xMin < b.xMax;
        bool c2 = a.xMax > b.xMin;
        bool c3 = a.yMin < b.yMax;
        bool c4 = a.yMax > b.yMin;
        return c1 && c2 && c3 && c4;
    }

    public void Drawing(Vector3 screenPos, Camera camera = null, bool drawOutside = false)
    {
        if (null == camera)
        {
            camera = Camera.main;
        }
        Vector3 uvPos = SpriteHitPoint2UV(camera.ScreenToWorldPoint(screenPos));
        screenPos = new Vector3(uvPos.x * _canvasWidth, _canvasHeight - uvPos.y * _canvasHeight, 0f);
        if (!_isDrawing)
        {
            _isDrawing = true;
            _prevMousePosition = screenPos;
        }

        if (_isDrawing)
        {
            if (_paintType == PaintType.DrawColorfulLine)
            {
                Color currentColor = _paintColorful[_colorfulIndex];
                _penColor = Color.Lerp(_penColor, currentColor, Time.deltaTime * _colorChangeRate);
                _colorfulTime += Time.deltaTime * _colorChangeRate;
                if (_colorfulTime > 1f)
                {
                    _colorfulTime = 0f;
                    ++_colorfulIndex;
                    if (_colorfulIndex >= _paintColorful.Length)
                    {
                        _colorfulIndex = 0;
                    }
                }
                _penMat.color = _penColor;
            }
            else if (_paintType == PaintType.DrawLine)
            {
                _penMat.color = _penColor;
            }
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _canvasWidth, _canvasHeight, 0);
            RenderTexture.active = _renderTex;
            if (_useVectorGraphic)
            {
                VectorGraphicDraw(ref screenPos, ref _prevMousePosition, drawOutside);
            }
            else
            {
                LerpDraw(ref screenPos, ref _prevMousePosition, drawOutside);
            }
            RenderTexture.active = null;
            GL.PopMatrix();
            _prevMousePosition = screenPos;
        }
    }

    protected void VectorGraphicDraw(ref Vector3 current, ref Vector3 previous, bool drawOutside)
    {
        if (Vector3.Distance(current, previous) > 0)
        {
            float radius = null != _penTex ? _penTex.width * _brushScale * 0.5f : _brushScale;
            _uv.width = _canvasWidth;
            _uv.height = _canvasHeight;
            Rect rect = new Rect(current.x - radius, current.y - radius, radius, radius);
            if (drawOutside || Intersect(ref _uv, ref rect))
            {
                _penMat.SetPass(0);

                float step = 0.2f;
                GL.Begin(GL.TRIANGLE_STRIP);
                GL.TexCoord2(0.5f, 0.5f);
                GL.Color(_penColor);
                for (float i = -step; i < 6.28318f; i += step)
                {
                    GL.Vertex3(previous.x, previous.y, 0f);
                    GL.Vertex3(previous.x + Mathf.Sin(i) * radius, previous.y + Mathf.Cos(i) * radius, 0f);
                    GL.Vertex3(previous.x + Mathf.Sin(i + step) * radius, previous.y + Mathf.Cos(i + step) * radius, 0f);

                    GL.Vertex3(current.x, current.y, 0f);
                    GL.Vertex3(current.x + Mathf.Sin(i) * radius, current.y + Mathf.Cos(i) * radius, 0f);
                    GL.Vertex3(current.x + Mathf.Sin(i + step) * radius, current.y + Mathf.Cos(i + step) * radius, 0f);
                }
                GL.End();

                GL.Begin(GL.QUADS);
                GL.TexCoord2(0.5f, 0.5f);
                GL.Color(_penColor);
                Vector3 dir = (current - previous).normalized;
                Vector3 normal = new Vector2(-dir.y, dir.x) * radius;
                GL.Vertex(previous + normal);
                GL.Vertex(previous - normal);
                GL.Vertex(current - normal);
                GL.Vertex(current + normal);
                GL.End();
            }
        }
	}

	protected void LerpDraw(ref Vector3 current, ref Vector3 previous, bool drawOutside)
	{
		float distance = Vector2.Distance(current, previous);
        if (distance > 0f)
		{
			Vector2 pos;
			float w = _penTex.width * _brushScale;
			float h = _penTex.height * _brushScale;
			float lerpDamp = Mathf.Min(w, h) * _drawLerpDamp;
            _uv.width = _canvasWidth;
            _uv.height = _canvasHeight;
            for (float i = 0; i < distance; i += lerpDamp)
            {
                float lDelta = i / distance;
                float lDifx = current.x - previous.x;
                float lDify = current.y - previous.y;
                pos.x = previous.x + (lDifx * lDelta);
                pos.y = previous.y + (lDify * lDelta);
                Rect rect = new Rect(pos.x - w * 0.5f, pos.y - h * 0.5f, w, h);
                if (drawOutside || Intersect(ref _uv, ref rect))
                {
                    Graphics.DrawTexture(rect, _penTex, _penMat);
                }
            }
		}
	}

	public void EndDraw()
    {
        _isDrawing = false;
    }

    public void ClearCanvas()
    {
        if (_renderTex)
        {
            Graphics.SetRenderTarget(_renderTex);
            Color color = new Color(0, 0, 0, 0);
            GL.Clear(true, true, color);
            RenderTexture.active = null;
        }
    }

    public void ShowScribbleComplete()
    {
        if (_paintType == PaintType.Scribble || _paintType == PaintType.None)
        {
            if (_isErase)
            {
                Graphics.SetRenderTarget(_renderTex);
                Color color = _canvasColor;
                color.a = 0f;
                GL.Clear(true, true, color);
            }
            else
            {
                if (_sourceTex)
                {
                    RenderTexture.active = _renderTex;
                    Graphics.Blit(_sourceTex, _renderTex);
                    RenderTexture.active = null;
                }
            }
        }
    }

    public void Dispose()
    {
        _isInited = false;
        if (_renderTex)
        {
            ResetCanvas();
            RenderTexture.active = null;
            _renderTex.Release();
            _renderTex = null;
        }
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (null != meshFilter)
        {
            Destroy(meshFilter);
        }
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (null != meshRenderer)
        {
            Destroy(meshRenderer);
        }
        if (_canvasMat)
        {
            Destroy(_canvasMat);
        }
        if (_penMat)
        {
            Destroy(_penMat);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        float w = _canvasWidth;
        float h = _canvasHeight;
        if (_useSourceTexSize && _sourceTex)
        {
            w = _sourceTex.width;
            h = _sourceTex.height;
        }

        Gizmos.color = _gizmosColor;
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(w * 0.01f, h * 0.01f, 0.1f));
        Gizmos.matrix = oldGizmosMatrix;
    }
#endif

}
