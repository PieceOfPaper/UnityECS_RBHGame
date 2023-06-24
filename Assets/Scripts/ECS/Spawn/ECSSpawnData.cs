using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public enum ECSSpawnRangeType : byte
{
    Circle, //radius, angle
    Box, //x, z
}

public partial struct ECSSpawnData : IComponentData
{
    public bool isEnable;
    public int spawnID;
    public float delay;

    public EntityData EntityData1;
    public EntityData EntityData2;
    public EntityData EntityData3;
    public float spawnDelay;
    public int spawnCount;
    
    public ECSSpawnRangeType rangeType;
    public float rangeArg1;
    public float rangeArg2;

    public struct EntityData
    {
        public Entity entity;
        public int rate;
    }


    public float delayTimer;
    public float spawnTimer;
    public Random random;
    public int spawnedCount;
}
