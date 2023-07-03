using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct ECSPlayerData : IComponentData
{
    public float expGatherRadius;
    
    public int level;
    public int exp;
}
