using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public struct ECSCharacterSpawnData
{
    public ECSCharacterData characterData;
    public LocalTransform transformData;
}

public class ECSCharacterSpawnManager : SingletonTemplate<ECSCharacterSpawnManager>
{
    private bool m_IsInitialized = false;
    private Queue<ECSCharacterSpawnData> m_QueuedSpawnData = new Queue<ECSCharacterSpawnData>();

    public void Initialie()
    {
        if (m_IsInitialized == false)
            return;

        m_IsInitialized = true;
    }

    public void EnqueueSpawnData(ECSCharacterSpawnData data) => m_QueuedSpawnData.Enqueue(data);

    public ECSCharacterSpawnData DequeueSpawnData() => m_QueuedSpawnData.Dequeue();

    public int QueuedSpawnDataCount() => m_QueuedSpawnData.Count;
    public bool HasQueuedSpawnData() => m_QueuedSpawnData.Count > 0;
}

public partial struct ECSCharacterSpawnSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        ECSCharacterSpawnManager.Instance.Initialie();
    }

    public void OnDestroy(ref SystemState state)
    {
        // throw new System.NotImplementedException();
    }
    
    [BurstCompile]
    private struct SpawnCharacterJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ecbParallelWriter;
        [ReadOnly] public NativeArray<ECSCharacterSpawnData> spawnDatas;

        public void Execute(int index)
        {
            var spawnCharacterData = spawnDatas[index];
            var entity = ecbParallelWriter.CreateEntity(0);
            ecbParallelWriter.AddComponent(0, entity, typeof(LocalTransform));
            ecbParallelWriter.SetComponent(0, entity, spawnCharacterData.transformData);
            ecbParallelWriter.AddComponent(0, entity, typeof(ECSCharacterData));
            ecbParallelWriter.SetComponent(0, entity, spawnCharacterData.characterData);
            ecbParallelWriter.AddComponent(0, entity, typeof(ECSMoveData));
            ecbParallelWriter.SetComponent(0, entity, new ECSMoveData()
            {
                speed = 1.0f,
                accel = 1.0f,
            });
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var ecbParallelWriter = ecb.AsParallelWriter();
        var spawnDatas = new NativeArray<ECSCharacterSpawnData>(ECSCharacterSpawnManager.Instance.QueuedSpawnDataCount(), Allocator.TempJob);

        int i = 0;
        while (ECSCharacterSpawnManager.Instance.HasQueuedSpawnData())
        {
            if (i >= spawnDatas.Length) break;
            spawnDatas[i] = ECSCharacterSpawnManager.Instance.DequeueSpawnData();
            i ++;
        }

        var job = new SpawnCharacterJob()
        {
            ecbParallelWriter = ecbParallelWriter,
            spawnDatas = spawnDatas,
        };
        var handle = job.Schedule(spawnDatas.Length, 10);
        handle.Complete();
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        spawnDatas.Dispose();
    }
}
