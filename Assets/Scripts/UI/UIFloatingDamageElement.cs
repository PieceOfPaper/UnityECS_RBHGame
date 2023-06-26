using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class UIFloatingDamageElement : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI m_TextDamage;
    
    private UIFloatingDamage m_Parent;
    private RectTransform m_ParentRectTransform;
    
    public ECSFloatingDamageData Data { get; private set; }

    private RectTransform m_RectTransform;
    public RectTransform MyRectTransform
    {
        get
        {
            if (m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
            return m_RectTransform;
        }
    }


    public void SetParent(UIFloatingDamage parent)
    {
        m_Parent = parent;
        m_ParentRectTransform = parent.GetComponent<RectTransform>();
    }
    
    public void PlayFloating(ECSFloatingDamageData data)
    {
        Data = data;
        if (m_TextDamage != null) m_TextDamage.text = data.damage.ToString();
        
        gameObject.SetActive(true);
    }

    public void UpdatePosition(Camera camera)
    {
        var screenPoint = camera.WorldToScreenPoint(Data.position);
        
        var localPoint = Vector2.zero;
        if (m_Parent == null || m_Parent.ParentCanvas == null || m_Parent.ParentCanvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, screenPoint, null, out localPoint);
        else if (m_Parent.ParentCanvas.rootCanvas.renderMode == RenderMode.ScreenSpaceCamera)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, screenPoint, m_Parent.ParentCanvas.rootCanvas.worldCamera, out localPoint);

        MyRectTransform.anchoredPosition = localPoint;
    }

    private void OnAnimationEnd()
    {
        if (m_Parent == null)
        {
            Destroy(gameObject);
            return;
        }
        
        m_Parent.EnqueueElement(this);
    }
}
