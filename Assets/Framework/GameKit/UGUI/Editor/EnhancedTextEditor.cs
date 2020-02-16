using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(EnhancedText))]
public class RichTextEditor : UnityEditor.UI.TextEditor
{
    public override void OnInspectorGUI()
    {
        EnhancedText component = (EnhancedText)target;
        base.OnInspectorGUI();
        component._isUseLocalize = EditorGUILayout.Toggle("Is Use Localize", component._isUseLocalize);
        if (component._isUseLocalize)
        {
            component._key = EditorGUILayout.TextField("Key", component._key);
            component._customFont = (EnhancedFont)EditorGUILayout.ObjectField("Custom Font", component._customFont, typeof(EnhancedFont), true);
        }
    }
}
