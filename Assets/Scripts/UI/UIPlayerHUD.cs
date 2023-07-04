using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIPlayerHUD : MonoBehaviour
{
    [Header("Pivot")]
    [SerializeField] private RectTransform m_Pivot;
    [SerializeField] private float m_Offset;
    
    [Header("Level / Exp")]
    [SerializeField] private TMPro.TextMeshProUGUI m_TextLevel;
    [SerializeField] private Slider m_SliderExpGauge;
    [SerializeField] private TMPro.TextMeshProUGUI m_TextExp;
    
    [Header("HP")]
    [SerializeField] private Slider m_SliderHPGauge;
    [SerializeField] private TMPro.TextMeshProUGUI m_TextHP;
    
    [Header("Reload")]
    [SerializeField] private Slider m_SliderReloadGauge;
    [SerializeField] private TMPro.TextMeshProUGUI m_TextReloadCount;

    private Canvas m_Canvas;
    private RectTransform m_RectTransform;
    private PlayerLevelData m_PlayerLevelData;

    private void Awake()
    {
        m_Canvas = GetComponentInParent<Canvas>();
        m_RectTransform = GetComponent<RectTransform>();
        m_PlayerLevelData = Resources.Load<PlayerLevelData>("Data/PlayerLevelData");
        
        if (m_Pivot != null) m_Pivot.anchoredPosition = Vector2.up * 10000f;
        if (m_TextLevel != null) m_TextLevel.text = string.Empty;
        if (m_SliderExpGauge != null) m_SliderExpGauge.value = 0f;
        if (m_TextExp != null) m_TextExp.text = string.Empty;
        if (m_SliderHPGauge != null) m_SliderHPGauge.value = 0f;
        if (m_TextHP != null) m_TextHP.text = string.Empty;
        if (m_SliderReloadGauge != null) m_SliderReloadGauge.value = 0f;
        if (m_TextReloadCount != null) m_TextReloadCount.text = string.Empty;
    }

    private void LateUpdate()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(ECSPlayerData), typeof(ECSCharacterData), typeof(ECSShootableData), typeof(ECSBindTransform));
        if (query.IsEmpty == false)
        {
            var entity = query.GetSingletonEntity();
            var playerData = entityManager.GetComponentData<ECSPlayerData>(entity);
            var characterData = entityManager.GetComponentData<ECSCharacterData>(entity);
            var shootableData = entityManager.GetComponentData<ECSShootableData>(entity);
            var bindTransform = entityManager.GetComponentObject<ECSBindTransform>(entity);

            var levelData = m_PlayerLevelData.GetDataByExp(playerData.exp);
            var nextLevelData = m_PlayerLevelData.GetDataByLevel(levelData.level + 1);
            
            if (m_Pivot != null)
            {
                if (Camera.main != null && bindTransform != null && bindTransform.transform != null)
                {
                    var screenPoint = Camera.main.WorldToScreenPoint(bindTransform.transform.position + Vector3.up * m_Offset);
        
                    var localPoint = Vector2.zero;
                    if (m_Canvas == null || m_Canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, screenPoint, null, out localPoint);
                    else if (m_Canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, screenPoint, m_Canvas.rootCanvas.worldCamera, out localPoint);

                    m_Pivot.anchoredPosition = localPoint;
                }
                else
                {
                    m_Pivot.anchoredPosition = Vector2.zero;
                }
            }
            if (m_TextLevel != null) m_TextLevel.text = string.Format("Lv.{0}", levelData.level);
            if (m_SliderExpGauge != null) m_SliderExpGauge.value = (float)(playerData.exp - levelData.requireExp) / (float)(nextLevelData.requireExp - levelData.requireExp);
            if (m_TextExp != null) m_TextExp.text = string.Format("{0}/{1}", playerData.exp - levelData.requireExp, nextLevelData.requireExp - levelData.requireExp);
            if (m_SliderHPGauge != null) m_SliderHPGauge.value = (float)characterData.hp / characterData.maxHp;
            if (m_TextHP != null) m_TextHP.text = string.Format("{0}/{1}", characterData.hp, characterData.maxHp);
            if (m_SliderReloadGauge != null) m_SliderReloadGauge.value = (shootableData.reloadTime - shootableData.currentReloadTime) / shootableData.reloadTime;
            if (m_TextReloadCount != null) m_TextReloadCount.text = string.Format("{0}/{1}", shootableData.remainShootCount, shootableData.reloadCount);
        }
    }
}
