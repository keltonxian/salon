using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;

//[CanEditMultipleObjects]
[CustomEditor(typeof(UIButton))]
public class UIButtonEditor : Editor
{
    private UIButton _target;

    void OnEnable()
    {
        _target = target as UIButton;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_effectType"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_clickInterval"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_eventType"), true);
        if (_target._eventType == UIButton.EventType.TOUCH)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onTouchDown"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onTouchUp"), true);
        }
        else if (_target._eventType == UIButton.EventType.CLICK)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_onClick"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }

}
