using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ECSBulletAuthoring))]
public class ECSBulletAuthoringInspector : Editor
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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("radius"));
        if (targets.Length == 1)
        {
            var script = target as ECSBulletAuthoring;
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
