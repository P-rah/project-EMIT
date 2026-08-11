using UnityEngine;

public class BoseBaseEmitController : EnemyEmitController<Boss1>
{
    public override void BeginEmit()
    {
        base.BeginEmit();
        emitableTarget.ChangeState(emitableTarget.StateMachine.MoveState, true);
    }
}
