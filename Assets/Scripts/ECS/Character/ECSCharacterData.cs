using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public partial struct ECSCharacterData : IComponentData
{
    public int hp;
    public int maxHp;

    public static ECSCharacterData Create(int hp)
    {
        var data = new ECSCharacterData()
        {
            hp = hp,
            maxHp = hp,
        };
        return data;
    }
}
