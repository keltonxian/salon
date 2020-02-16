using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(UGUIDrag))]
[CanEditMultipleObjects]
public class UGUIDragEditor : Editor
{
    public override void OnInspectorGUI()
    {
        UGUIDrag drager = target as UGUIDrag;
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_tool"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_box"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_raycastCamera"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_raycastMask"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_raycastDepth"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isDragOriginPoint"), false);
        if (drager._isDragOriginPoint)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragOffset"), false);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragOffsetZ"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragChangeScale"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragChangeRotate"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_draggingParent"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isDragOnPointerDown"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerPos"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerType"), false);
        if (drager._triggerType == UGUIDrag.TriggerType.Circle)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerRadius"), false);
        }
        else if (drager._triggerType == UGUIDrag.TriggerType.Range)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_triggerRange"), false);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_soundDragging"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_particleDragging"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragEffectType"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_dragValidType"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_pageView"), false);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_isReleaseAutoBack"), false);
        if (drager._isReleaseAutoBack)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_backEffect"), false);
            if (drager._backEffect != UGUIDrag.DragBackEffect.None &&
                drager._backEffect != UGUIDrag.DragBackEffect.Destroy &&
                drager._backEffect != UGUIDrag.DragBackEffect.Immediately)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_backDuring"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_tweenEase"), false);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_isBackKeepTop"), false);
            }
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_hasTip"), false);
        if (drager._hasTip)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_tipBeforeDrag"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_tipDragging"), false);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_hasBtn"), false);
        if (drager._hasBtn)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_btnClick"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_soundClick"), false);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_hasAudio"), false);
        if (drager._hasAudio)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_soundStartDrag"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_soundEndDrag"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_voStartDrag"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_voEndDrag"), false);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_voDone"), false);
        }
        serializedObject.ApplyModifiedProperties();
    }
}
