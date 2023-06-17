using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public partial struct ECSCharacterData : IComponentData
{
    public int resourceID;
    public int hp;
    public int maxHp;

    public static ECSCharacterData Create(int resourceID, int hp)
    {
        var data = new ECSCharacterData()
        {
            resourceID = resourceID,
            hp = hp,
            maxHp = hp,
        };
        return data;
    }
}
