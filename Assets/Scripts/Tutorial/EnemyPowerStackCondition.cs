using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Cond_Power_", menuName = "Tutorial/Condition/Power")]
public class EnemyPowerStackCondition : TutorialCondition
{
    [SerializeField] private int targetPowerStack;

    public override bool IsMet(EnemyBase enemy)
    {
        return enemy != null && 
            (enemy.currentPower == targetPowerStack || 
            enemy.currentPower == -targetPowerStack);
    }
}