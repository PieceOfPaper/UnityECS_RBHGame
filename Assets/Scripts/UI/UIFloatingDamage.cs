using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIFloatingDamage : MonoBehaviour
{
    [SerializeField] private UIFloatingDamageElement m_Element;

    private Canvas m_ParentCanvas;
    public Canvas ParentCanvas => m_ParentCanvas;

    private Queue<UIFloatingDamageElement> m_QueuedElements = new Queue<UIFloatingDamageElement>();
    private List<UIFloatingDamageElement> m_PlayedElements = new List<UIFloatingDamageElement>();

    private void Awake()
    {
        if (m_Element != null) m_Element.gameObject.SetActive(false);
    }
    
    private void OnEnable()
    {
        m_ParentCanvas = GetComponentInParent<Canvas>();
    }

    private void OnDisable()
    {
        m_ParentCanvas = null;
    }

    private void LateUpdate()
    {
        bool needSort = false;

        
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(ECSFloatingDamageData));
        if (query.IsEmpty == false)
        {
            var floatingDamageEntities = query.ToEntityArray(Allocator.Temp);
            var floatingDamageDatas = query.ToComponentDataArray<ECSFloatingDamageData>(Allocator.Temp);
            for (int i = 0; i < floatingDamageDatas.Length; i ++)
            {
                UIFloatingDamageElement element = null;
                if (m_QueuedElements.Count == 0)
                {
                    if (m_Element != null)
                    {
                        element = Instantiate(m_Element, m_Element.transform.parent);
                        element.SetParent(this);
                    }
                }
                else
                {
                    element = m_QueuedElements.Dequeue();
                }
                if (element == null) continue;

                element.PlayFloating(floatingDamageDatas[i]);
                m_PlayedElements.Add(element);
                needSort = true;
                
                entityManager.DestroyEntity(floatingDamageEntities[i]);
            }
            floatingDamageEntities.Dispose();
            floatingDamageDatas.Dispose();
        }


        if (m_PlayedElements.Count > 0)
        {
            var mainCamera = Camera.main;
            for (int i = 0; i < m_PlayedElements.Count; i ++)
            {
                m_PlayedElements[i].UpdatePosition(mainCamera);
            }
            if (needSort == true)
            {
                m_PlayedElements.Sort((a, b) => a.MyRectTransform.anchoredPosition.y.CompareTo(b.MyRectTransform.anchoredPosition.y));
                for (int i = 0; i < m_PlayedElements.Count; i ++)
                {
                    m_PlayedElements[i].MyRectTransform.SetAsLastSibling();
                }
            }
        }
    }


    public void EnqueueElement(UIFloatingDamageElement element)
    {
        element.gameObject.SetActive(false);
        m_QueuedElements.Enqueue(element);
        m_PlayedElements.Remove(element);
    }
}
