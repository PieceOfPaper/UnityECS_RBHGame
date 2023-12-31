using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// 입력 그룹.
/// Input뿐만 아니라 AI도 여기서 동작하도록 한다.
/// </summary>
public partial class ECSInputSystemGroup : ComponentSystemGroup { }

/// <summary>
/// Entity를 생성하는 그룹.
/// </summary>
[UpdateAfter(typeof(ECSInputSystemGroup))]
public partial class ECSSpawnSystemGroup : ComponentSystemGroup { }

/// <summary>
/// 아무튼 사전에 처리하는 그룹.
/// </summary>
[UpdateAfter(typeof(ECSSpawnSystemGroup))]
public partial class ECSPreProcessSystemGroup : ComponentSystemGroup { }

/// <summary>
/// 처리하는 그룹.
/// </summary>
[UpdateAfter(typeof(ECSPreProcessSystemGroup))]
public partial class ECSProcessSystemGroup : ComponentSystemGroup { }

/// <summary>
/// 아무튼 나중에 처리하는 그룹
/// </summary>
[UpdateAfter(typeof(ECSProcessSystemGroup))]
public partial class ECSAfterProcessSystemGroup : ComponentSystemGroup { }

/// <summary>
/// 실제 오브젝트 처리하는 그룹
/// </summary>
[UpdateAfter(typeof(ECSAfterProcessSystemGroup))]
public partial class ECSBindProcessSystemGroup : ComponentSystemGroup { }