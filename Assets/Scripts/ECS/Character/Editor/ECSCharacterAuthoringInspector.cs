using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ECSCharacterAuthoring))]
public class ECSCharacterAuthoringInspector : Editor
{
    private string[] m_CharacterLayerNames = null;
    
    private void OnEnable()
    {
        m_CharacterLayerNames = System.Enum.GetNames(typeof(ECSCharacterLayer));
    }

    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("hp"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("layer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("damagedCooltime"));
        if (targets.Length == 1)
        {
            var script = target as ECSCharacterAuthoring;
            var attackableLayer = EditorGUILayout.MaskField("Attackable Layer", script.attackableLayer, m_CharacterLayerNames);
            if (script.attackableLayer != attackableLayer)
            {
                script.attackableLayer = attackableLayer;
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackableLayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackDamage"));
        
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
