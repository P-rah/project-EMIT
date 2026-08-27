using UnityEngine;

public class AttackStateMonitor : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Player.Combat.AttackSweepReset();
    }
}
