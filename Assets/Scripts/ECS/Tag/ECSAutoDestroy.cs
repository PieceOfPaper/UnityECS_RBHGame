using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public partial struct ECSAutoDestroy : IComponentData
{
    public float delayTime;
    
    public float currentDelayTime;
}
