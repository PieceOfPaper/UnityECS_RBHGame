using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingletonTemplate<T> where T : class, new()
{
    static T m_Instance = null;
    public static T Instance
    {
        get
        {
            if (m_Instance == null) m_Instance = new T();
            return m_Instance;
        }
    }

    public static void ResetInstance() { m_Instance = null; }
}

public abstract class MonoBehaviourSingletonTemplate<T> : MonoBehaviour where T : MonoBehaviour
{
    static T m_Instance = null;
    public static T Instance
    {
        get
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode == false)
                return null;
#endif
            if (m_Instance == null)
            {
                var obj = new GameObject(typeof(T).Name);
                m_Instance = obj.AddComponent<T>();
            }
            return m_Instance;
        }
    }

    public static void ResetInstance() 
    {
        if (m_Instance == null) return;
        Destroy(m_Instance.gameObject);
        m_Instance = null;
    }
    
    protected virtual void Awake()
    {
    	DontDestroyOnLoad(gameObject);
    }
}