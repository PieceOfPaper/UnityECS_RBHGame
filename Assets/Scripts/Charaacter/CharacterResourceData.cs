using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterResourceData : ScriptableObject
{
    [SerializeField] private Data[] m_Datas;

    [System.Serializable]
    public struct Data
    {
        public int resourceID;
        public string prefabName;
    }

    private Dictionary<int, string> m_CachedData;
    public string GetPrefabName(int resourceID)
    {
#if UNITY_EDITOR
        if (Application.isPlaying == false)
        {
            for (int i = 0; i < m_Datas.Length; i ++)
            {
                if (m_Datas[i].resourceID == resourceID)
                    return m_Datas[i].prefabName;
            }
            return string.Empty;
        }
#endif

        if (m_CachedData == null)
        {
            m_CachedData = new Dictionary<int, string>();
            for (int i = 0; i < m_Datas.Length; i ++)
            {
                if (m_CachedData.ContainsKey(m_Datas[i].resourceID)) continue;

                m_CachedData.Add(m_Datas[i].resourceID, m_Datas[i].prefabName);
            }
        }

        if (m_CachedData.ContainsKey(resourceID))
            return m_CachedData[resourceID];

        return string.Empty;
    }

#if UNITY_EDITOR

    [UnityEditor.MenuItem("Tools/Create Character Resource Data")]
    public static void Create()
    {
        var data = ScriptableObject.CreateInstance<CharacterResourceData>();
        UnityEditor.AssetDatabase.CreateAsset(data, "Assets/Resources/CharacterResourceData.asset");
    }

#endif
}
