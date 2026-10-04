using System;
using UnityEngine;

[Serializable]
public abstract class TutorialCondition : ScriptableObject
{
    // 해당 조건이 충족되었는지 검사하는 메서드
    public abstract bool IsMet(EnemyBase enemy);

}