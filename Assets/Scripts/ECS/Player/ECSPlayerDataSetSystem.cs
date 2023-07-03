using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(ECSPreProcessSystemGroup))]
public partial struct ECSPlayerDataSetSystem : ISystem
{
    private static PlayerLevelData playerLevelData;
    
    public void OnCreate(ref SystemState state)
    {
        playerLevelData = Resources.Load<PlayerLevelData>("Data/PlayerLevelData");
    }

    public void OnDestroy(ref SystemState state)
    {
        if (playerLevelData != null) Resources.UnloadAsset(playerLevelData);
    }

    public void OnUpdate(ref SystemState state)
    {
        if (playerLevelData != null)
        {
            foreach (var (refPlayerData, refShootableData) in SystemAPI.Query<RefRO<ECSPlayerData>, RefRW<ECSShootableData>>())
            {
                var levelData = playerLevelData.GetDataByExp(refPlayerData.ValueRO.exp);
                var shootableData = refShootableData.ValueRO;
                shootableData.shootCount = levelData.shootCount;
                shootableData.shootCooltime = levelData.shootCooltime;
                shootableData.reloadCount = levelData.reloadCount;
                shootableData.reloadTime = levelData.reloadTime;
                refShootableData.ValueRW = shootableData;
            }
        }
    }
}
