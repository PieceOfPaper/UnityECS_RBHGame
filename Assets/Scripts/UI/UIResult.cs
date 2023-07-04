using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIResult : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI m_TextPlayTime;
    [SerializeField] private TMPro.TextMeshProUGUI m_TextLevel;

    private PlayerLevelData m_PlayerLevelData;
    
    public void SetData(float playTime, ECSPlayerData playerData)
    {
        if (m_PlayerLevelData == null)
            m_PlayerLevelData = Resources.Load<PlayerLevelData>("Data/PlayerLevelData");
        
        var timespan = System.TimeSpan.FromSeconds(playTime);
        var levelData = m_PlayerLevelData.GetDataByExp(playerData.exp);

        if (m_TextPlayTime != null) m_TextPlayTime.text = string.Format("{0:00}:{1:00}:{2:00}", (int)timespan.TotalMinutes, timespan.Seconds, timespan.Milliseconds);
        if (m_TextLevel != null) m_TextLevel.text = levelData.level.ToString();
    }
}
