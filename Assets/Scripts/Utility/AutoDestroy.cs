using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public enum DestroyAtType
    {
        OnAwake,
        OnEnable,
        OnDisable,
        OnStart,
    }

    public DestroyAtType destroyAt = DestroyAtType.OnAwake;
    public float delay = 0f;


    private bool m_IsDestroyed = false;
    
    private void Awake()
    {
        if (destroyAt == DestroyAtType.OnAwake) CallDestroy();
    }

    private void OnDestroy()
    {
        m_IsDestroyed = true;
    }

    private void OnEnable()
    {
        if (destroyAt == DestroyAtType.OnEnable) CallDestroy();
    }
    
    private void OnDisable()
    {
        if (destroyAt == DestroyAtType.OnDisable) CallDestroy();
    }
    
    private void Start()
    {
        if (destroyAt == DestroyAtType.OnStart) CallDestroy();
    }

    public void CallDestroy()
    {
        Invoke("DoDestroy", delay);
    }
    
    public void DoDestroy()
    {
        if (m_IsDestroyed == true)
            return;
        
        Destroy(gameObject);
        m_IsDestroyed = true;
    }
}
