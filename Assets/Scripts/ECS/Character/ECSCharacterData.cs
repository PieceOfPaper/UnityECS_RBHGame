using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public partial struct ECSCharacterData : IComponentData
{
    public int hp;
    public int maxHp;
}