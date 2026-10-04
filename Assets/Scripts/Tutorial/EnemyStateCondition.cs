using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Cond_State_", menuName = "Tutorial/Condition/State")]
public class EnemyStateCondition : TutorialCondition
{
    [SerializeField] private String targetStateString;

    public override bool IsMet(EnemyBase enemy)
    {
        return enemy != null 
            && enemy.CurrentStateToString() != null 
            && enemy.CurrentStateToString() == targetStateString;
    }
}
