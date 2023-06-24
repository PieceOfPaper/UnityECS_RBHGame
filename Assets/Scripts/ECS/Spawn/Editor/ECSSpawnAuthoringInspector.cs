using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ECSSpawnAuthoring))]
public class ECSSpawnAuthoringInspector : Editor
{
    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isEnable"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("delay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnEntityDatas"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnCount"));

        ECSSpawnRangeType[] prevRangeTypes = new ECSSpawnRangeType[targets.Length];
        for (int i = 0; i < targets.Length; i ++)
        {
            if (targets[i] is ECSSpawnAuthoring authoring)
            {
                prevRangeTypes[i] = authoring.rangeType;
            }
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rangeType"));
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
        for (int i = 0; i < targets.Length; i ++)
        {
            if (targets[i] is ECSSpawnAuthoring authoring)
            {
                if (prevRangeTypes[i] != authoring.rangeType)
                {
                    switch (authoring.rangeType)
                    {
                        case ECSSpawnRangeType.Circle:
                            authoring.rangeArg1 = 0.5f;
                            authoring.rangeArg2 = 360.0f;
                            break;
                        case ECSSpawnRangeType.Box:
                            authoring.rangeArg1 = 1.0f;
                            authoring.rangeArg2 = 1.0f;
                            break;
                    }
                }
            }
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rangeArg1"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rangeArg2"));

        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
