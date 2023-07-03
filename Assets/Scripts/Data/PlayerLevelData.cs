using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelData : ScriptableObject
{
    public List<Data> datas = new List<Data>();
    
    [System.Serializable]
    public struct Data
    {
        public int level;
        public int requireExp;
        
        public int shootCount;
        public float shootCooltime;
        public int reloadCount;
        public float reloadTime;
    }

    private Dictionary<int, Data> levelToData = null;

    public Data GetDataByLevel(int level)
    {
        if (Application.isPlaying == true)
        {
            if (levelToData == null)
            {
                levelToData = new Dictionary<int, Data>();
                for (int i = 0; i < datas.Count; i ++)
                {
                    if (levelToData.ContainsKey(datas[i].level)) continue;
                    levelToData.Add(datas[i].level, datas[i]);
                }
            }
            if (levelToData.ContainsKey(level))
                return levelToData[level];
        }
        else
        {
            for (int i = 0; i < datas.Count; i ++)
            {
                if (datas[i].level == level)
                    return datas[i];
            }
        }
        return default;
    }

    public Data GetDataByExp(int exp)
    {
        for (int i = 0; i < datas.Count; i ++)
        {
            if (exp < datas[i].requireExp)
                return i > 0 ? datas[i - 1] : default;
        }
        return default;
    }
    
    #if UNITY_EDITOR

    [UnityEditor.MenuItem("Tools/Data/Create PlayerLevelData")]
    public static void CreateNewData()
    {
        var newAsset = ScriptableObject.CreateInstance<PlayerLevelData>();
        UnityEditor.AssetDatabase.CreateAsset(newAsset, "Assets/Resources/Data/PlayerLevelData.asset");
    }
    
    #endif
}
