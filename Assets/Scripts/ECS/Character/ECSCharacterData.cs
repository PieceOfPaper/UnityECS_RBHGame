using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public enum ECSCharacterLayer : byte
{
    None = 0,
    Player,
    Enemy,
}

public partial struct ECSCharacterData : IComponentData
{
    public int hp;
    public int maxHp;
    public float radius;
    public ECSCharacterLayer layer;
    public float damagedCooltime;
    public Entity deadEffect;
    
    public int attackableLayer;
    public int attackDamage;
    public Entity hitEffect;

    public Entity dropItem;


    public float damagedTimer;
    public bool isDead;
}