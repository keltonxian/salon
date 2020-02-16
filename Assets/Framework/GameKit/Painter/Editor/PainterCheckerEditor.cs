using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(PainterChecker))]
[CanEditMultipleObjects]
public class PainterCheckerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PainterChecker checker = target as PainterChecker;
        RenderTexturePainter painter = checker.gameObject.GetComponent<RenderTexturePainter>();

        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_gridDefaultStatus"), true);
        if (painter == null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sourceTex"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_penTex"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_gridScale"), true);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_enableColor"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_disableColor"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_checkData"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_canResetData"), true);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_editorBrushSize"), true);
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("(Ctrl+)Read Data"))
        {
            ReadGrid();
        }
        SpriteRenderer sr = checker.gameObject.GetComponent<SpriteRenderer>();
        if (sr == null && GUILayout.Button("Show Source Tex"))
        {
            Texture2D tex = checker._sourceTex as Texture2D;
            if (painter && painter.SourceTex)
            {
                tex = painter.SourceTex as Texture2D;
            }
            if (tex)
            {
                SpriteRenderer render = checker.GetComponent<SpriteRenderer>();
                if (!render) render = checker.gameObject.AddComponent<SpriteRenderer>();
                render.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
            }
        }
        else if (sr && GUILayout.Button("Remove SpriteRenderer"))
        {
            DestroyImmediate(sr);
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("(Ctrl+)Create Data"))
        {
            CreateGrid();
        }
        if (GUILayout.Button("Save Grid Data"))
        {
            SaveDataToFile();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.TextArea("Cmd/Ctrl + Mouse: Add Point\nAlt + Mouse : Remove Point");
    }

    void CreateGrid()
    {
        PainterChecker checker = target as PainterChecker;
        RenderTexturePainter painter = checker.gameObject.GetComponent<RenderTexturePainter>();

        Texture2D tex = checker._sourceTex as Texture2D;
        if (painter && painter.SourceTex)
        {
            tex = painter.SourceTex as Texture2D;
        }
        Texture2D pen = checker._penTex as Texture2D;
        if (painter && painter._penTex != null)
        {
            pen = painter._penTex as Texture2D;
        }

        if (tex && pen)
        {
            checker._dicGrid = new Dictionary<string, Rect>();
            checker._dicEnable = new Dictionary<string, bool>();

            Vector2 gridSize = GetGridSize();
            int gridW = (int)(gridSize.x);
            int gridH = (int)(gridSize.y);

            int canvasW = tex.width;
            int canvasH = tex.height;

            for (int w = -canvasW / 2; w <= canvasW / 2; w += gridW)
            {
                for (int h = -canvasH / 2; h <= canvasH / 2; h += gridH)
                {
                    string key = w * 0.01f + "-" + h * 0.01f;
                    Rect value = new Rect(w * 0.01f, h * 0.01f, gridH * 0.01f, gridW * 0.01f);
                    checker._dicGrid[key] = value;
                    checker._dicEnable[key] = checker._gridDefaultStatus;
                }
            }
        }
    }

    void SaveDataToFile()
    {
        //序列化存储
        PainterChecker checker = target as PainterChecker;
        if (checker._dicGrid != null)
        {
            if (checker._checkData == null)
            {
                AssetDatabase.CreateAsset(GetGridData(), "Assets/" + checker.name + "_ScribbleCheckData.asset");
            }
            else
            {
                AssetDatabase.Refresh();
                PaintCheckData checkData = checker._checkData;
                checkData.checkPoints = new List<Vector2>();
                checkData.gridSize = GetGridSize();
                foreach (string key in checker._dicGrid.Keys)
                {
                    if (checker._dicEnable[key])
                    {
                        Rect r = checker._dicGrid[key];
                        checkData.checkPoints.Add(r.center);
                    }
                }
                EditorUtility.SetDirty(checker._checkData);
                AssetDatabase.SaveAssets();
            }
        }
    }

    PaintCheckData GetGridData()
    {
        PainterChecker checker = target as PainterChecker;
        PaintCheckData checkData = ScriptableObject.CreateInstance<PaintCheckData>();
        checkData.checkPoints = new List<Vector2>();
        checkData.gridSize = GetGridSize();
        foreach (string key in checker._dicGrid.Keys)
        {
            if (checker._dicEnable[key])
            {
                Rect r = checker._dicGrid[key];
                checkData.checkPoints.Add(r.center);
            }
        }
        return checkData;
    }

    Vector2 GetGridSize()
    {
        PainterChecker checker = target as PainterChecker;
        RenderTexturePainter painter = checker.gameObject.GetComponent<RenderTexturePainter>();

        Texture2D pen = checker._penTex as Texture2D;
        if (painter && painter._penTex != null)
        {
            pen = painter._penTex as Texture2D;
            int gridW = Mathf.FloorToInt(pen.width * painter._brushScale / 4f);
            int gridH = Mathf.FloorToInt(pen.height * painter._brushScale / 4f);
            return new Vector2(gridW, gridH);
        }
        if (pen)
        {
            int gridW = Mathf.FloorToInt(pen.width * checker._gridScale / 4f);
            int gridH = Mathf.FloorToInt(pen.height * checker._gridScale / 4f);
            return new Vector2(gridW, gridH);
        }
        return Vector2.one * checker._gridScale;
    }

    void ReadGrid()
    {
        PainterChecker checker = target as PainterChecker;
        if (checker._checkData != null)
        {
            checker._dicGrid = new Dictionary<string, Rect>();
            checker._dicEnable = new Dictionary<string, bool>();
            Vector2 gridSize = GetGridSize();
            foreach (Vector2 v in checker._checkData.checkPoints)
            {
                Rect rect = new Rect(v.x - gridSize.x * 0.005f, v.y - gridSize.y * 0.005f, gridSize.x * 0.01f, gridSize.y * 0.01f);
                string key = v.x + "-" + v.y;
                checker._dicGrid[key] = rect;
                checker._dicEnable[key] = true;
            }
        }
    }

    bool Intersect(ref Rect a, ref Rect b)
    {
        bool c1 = a.xMin < b.xMax;
        bool c2 = a.xMax > b.xMin;
        bool c3 = a.yMin < b.yMax;
        bool c4 = a.yMax > b.yMin;
        return c1 && c2 && c3 && c4;
    }

    void OnSceneGUI()
    {
        PainterChecker checker = target as PainterChecker;
        Handles.color = Color.blue;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event current = Event.current;
        if (checker._dicGrid != null && (current.control || current.command || current.alt))
        {
            switch (current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    current.Use();
                    break;
                case EventType.MouseMove:
                    Vector3 p = HandleUtility.GUIPointToWorldRay(current.mousePosition).origin;
                    Vector3 localPos = checker.transform.InverseTransformPoint(p);

                    Rect brushSize = new Rect(localPos.x - checker._editorBrushSize / 2f, localPos.y - checker._editorBrushSize / 2f, checker._editorBrushSize, checker._editorBrushSize);

                    if (current.control || current.command)
                    {
                        foreach (string key in checker._dicGrid.Keys)
                        {
                            Rect rect = checker._dicGrid[key];
                            if (Intersect(ref rect, ref brushSize))
                            {
                                checker._dicEnable[key] = true;
                            }
                        }
                    }
                    else if (current.alt)
                    {
                        foreach (string key in checker._dicGrid.Keys)
                        {
                            Rect rect = checker._dicGrid[key];
                            if (Intersect(ref rect, ref brushSize))
                            {
                                checker._dicEnable[key] = false;
                            }
                        }
                    }
                    Event.current.Use();
                    break;
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(controlID);
                    break;
            }
        }
    }
}
