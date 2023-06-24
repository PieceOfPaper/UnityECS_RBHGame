using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[RequireComponent(typeof(Cinemachine.CinemachineVirtualCameraBase))]
public class CinemachineVirtualCameraPlayerFollow : MonoBehaviour
{
    [SerializeField] public bool follow = true;
    [SerializeField] public bool lookAt = false;
    
    Cinemachine.CinemachineVirtualCameraBase m_VCam;
    public Cinemachine.CinemachineVirtualCameraBase VCam
    {
        get
        {
            if (m_VCam == null) m_VCam = GetComponent<Cinemachine.CinemachineVirtualCameraBase>();
            return m_VCam;
        }
    }

    private Transform m_TargetTrasnform;

    private void Start() 
    {
        VCam.Follow = null;
        VCam.LookAt = null;
    }

    private void LateUpdate()
    {
        if (m_TargetTrasnform == null)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityQuery query = entityManager.CreateEntityQuery(typeof(ECSPlayerTag));
            if (query.IsEmpty == false)
            {
                var entity = query.GetSingletonEntity();
                var bindTransform = entityManager.GetComponentData<ECSBindTransform>(entity);
                if (bindTransform != null && bindTransform.transform != null)
                {
                    m_TargetTrasnform = bindTransform.transform;
                }
            }
        }
        
        if (follow == true && VCam.Follow == null)
        {
            VCam.Follow = m_TargetTrasnform;
        }
        if (lookAt == true && VCam.LookAt == null)
        {
            VCam.LookAt = m_TargetTrasnform;
        }
    }
}
