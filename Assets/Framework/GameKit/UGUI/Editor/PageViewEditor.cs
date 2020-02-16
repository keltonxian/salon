using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(PageView), true)]
public class PageViewEditor : ScrollRectEditor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		(target as PageView).movementType = UnityEngine.UI.ScrollRect.MovementType.Unrestricted;
		serializedObject.Update();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_pageIndicator"), false);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_pageDamp"), false);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_isDragOutSideEnabled"), false);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_isAutoInit"), false);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_isLoop"), false);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_onScrollOver"), false);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("_onPageChange"), false);
		serializedObject.ApplyModifiedProperties();
	}
}
