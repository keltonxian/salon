using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

[CanEditMultipleObjects]
[CustomEditor(typeof(RenderTexturePainter))]
public class RenderTexturePainterEditor : Editor
{
    private RenderTexturePainter _painter;
    string[] _sortingLayerNames;
    int _selectedOption;

    void OnEnable()
    {
        _painter = target as RenderTexturePainter;
        _sortingLayerNames = GetSortingLayerNames();
        _selectedOption = GetSortingLayerIndex(_painter.SortingLayerName);
    }

    public string[] GetSortingLayerNames()
    {
        System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }

    public int[] GetSortingLayerUniqueIDs()
    {
        System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
        return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
    }

    protected int GetSortingLayerIndex(string layerName)
    {
        for (int i = 0; i < _sortingLayerNames.Length; i++)
        {
            if (_sortingLayerNames[i] == layerName)
            {
                return i;
            }
        }
        return 0;
    }

    public override void OnInspectorGUI()
    {
        if (_painter._paintType == RenderTexturePainter.PaintType.None)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_gizmosColor"), true);

            _selectedOption = EditorGUILayout.Popup("Sorting Layer", _selectedOption, _sortingLayerNames);
            if (_sortingLayerNames[_selectedOption] != _painter.SortingLayerName)
            {
                Undo.RecordObject(_painter, "Sorting Layer");
                _painter.SortingLayerName = _sortingLayerNames[_selectedOption];
                EditorUtility.SetDirty(_painter);
            }
            int newSortingLayerOrder = EditorGUILayout.IntField("Order In Layer", _painter.SortingOrder);
            if (newSortingLayerOrder != _painter.SortingOrder)
            {
                Undo.RecordObject(_painter, "Edit Sorting Order");
                _painter.SortingOrder = newSortingLayerOrder;
                EditorUtility.SetDirty(_painter);
            }
            if (!_painter._useSourceTexSize)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasWidth"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasHeight"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasColor"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_paintType"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_useVectorGraphic"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_penTex"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sourceTex"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_maskTex"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_paintShader"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_scribbleShader"), true);

            if (null == _painter.PaintShader)
            {
                _painter.PaintShader = Shader.Find("Painter/Paint Shader");
            }
            if (null == _painter.ScribbleShader)
            {
                _painter.ScribbleShader = Shader.Find("Painter/Scribble Shader");
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_cullMode"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_penColor"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_brushScale"), true);

            if (!_painter._useVectorGraphic)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_drawLerpDamp"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isErase"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_paintColorful"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_colorChangeRate"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isAutoInit"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isAutoDestroy"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_isShowSource"), true);
            serializedObject.ApplyModifiedProperties();

            return;
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_gizmosColor"), true);

        _selectedOption = EditorGUILayout.Popup("Sorting Layer", _selectedOption, _sortingLayerNames);
        if (_sortingLayerNames[_selectedOption] != _painter.SortingLayerName)
        {
            Undo.RecordObject(_painter, "Sorting Layer");
            _painter.SortingLayerName = _sortingLayerNames[_selectedOption];
            EditorUtility.SetDirty(_painter);
        }
        int sortingLayerOrder = EditorGUILayout.IntField("Order in Layer", _painter.SortingOrder);
        if (sortingLayerOrder != _painter.SortingOrder)
        {
            Undo.RecordObject(_painter, "Edit Sorting Order");
            _painter.SortingOrder = sortingLayerOrder;
            EditorUtility.SetDirty(_painter);
        }

        if (_painter._paintType == RenderTexturePainter.PaintType.Scribble || _painter._paintType == RenderTexturePainter.PaintType.None)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_useSourceTexSize"), true);
            if (!_painter._useSourceTexSize)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasWidth"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasHeight"), true);
            }
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasWidth"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasHeight"), true);
        }

        if (_painter._paintType == RenderTexturePainter.PaintType.Scribble)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_canvasColor"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_paintType"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_useVectorGraphic"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_penTex"), true);
        if (_painter._paintType == RenderTexturePainter.PaintType.Scribble)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sourceTex"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_maskTex"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_paintShader"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_scribbleShader"), true);
        if (null == _painter.PaintShader)
        {
            _painter.PaintShader = Shader.Find("Painter/Paint Shader");
        }
        if (null == _painter.ScribbleShader)
        {
            _painter.ScribbleShader = Shader.Find("Painter/Scribble Shader");
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_cullMode"), true);
        if (_painter._paintType == RenderTexturePainter.PaintType.DrawLine)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_penColor"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_brushScale"), true);
        if (!_painter._useVectorGraphic)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_drawLerpDamp"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isErase"), true);
        if (_painter._paintType == RenderTexturePainter.PaintType.DrawColorfulLine)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_paintColorful"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_colorChangeRate"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isAutoInit"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isAutoDestroy"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isShowSource"), true);
        serializedObject.ApplyModifiedProperties();
    }

}
