using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ECSWallAuthoring))]
public class ECSWallAuthoringInspector : Editor
{
    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        
        ECSRangeType[] prevRangeTypes = new ECSRangeType[targets.Length];
        for (int i = 0; i < targets.Length; i ++)
        {
            if (targets[i] is ECSWallAuthoring authoring)
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
            if (targets[i] is ECSWallAuthoring authoring)
            {
                if (prevRangeTypes[i] != authoring.rangeType)
                {
                    switch (authoring.rangeType)
                    {
                        case ECSRangeType.Circle:
                            authoring.rangeArg1 = 0.5f;
                            authoring.rangeArg2 = 360.0f;
                            break;
                        case ECSRangeType.Box:
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
