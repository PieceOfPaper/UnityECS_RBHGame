using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerLevelData))]
public class PlayerLevelDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var script = target as PlayerLevelData;
        if (GUILayout.Button("Sort"))
        {
            script.datas.Sort((a, b) => a.level.CompareTo(b.level));
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        using (new EditorGUI.DisabledScope(script.datas.Count == 0))
        {
            if (GUILayout.Button("Add Last"))
            {
                var lastData = script.datas[script.datas.Count - 1];
                script.datas.Add(new PlayerLevelData.Data() { level = lastData.level + 1 });
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
        if (GUILayout.Button("Generate 100"))
        {
            script.datas.Clear();

            int currentExp = 0;
            int currentShootCount = 1;
            float currentShootCooltime = 0.5f;
            int currentReloadCount = 6;
            float currentReloadTime = 1.0f;
            for (int i = 0; i < 100; i ++)
            {
                currentExp = currentExp + i * 1;
                currentShootCount = Mathf.Min(1 + i / 4, 12);
                currentShootCooltime = 0.3f * ((100f - i) / 100f);
                currentReloadCount = 6 + i / 8;
                currentReloadTime = 0.2f + 0.8f * ((100f - i) / 100f);
                script.datas.Add(new PlayerLevelData.Data()
                {
                    level = i + 1,
                    requireExp = currentExp,
                    shootCount = currentShootCount,
                    shootCooltime = currentShootCooltime,
                    reloadCount = currentReloadCount,
                    reloadTime = currentReloadTime,
                });
            }
            
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Clear"))
        {
            script.datas.Clear();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
