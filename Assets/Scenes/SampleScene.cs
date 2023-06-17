using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SampleScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ECSCharacterSpawnManager.Instance.EnqueueSpawnData(new ECSCharacterSpawnData()
            {
                characterData = ECSCharacterData.Create(100),
                transformData = new LocalTransform() { Position = new float3(), Rotation = quaternion.identity, Scale = 1f },
            });
        }
    }
}
