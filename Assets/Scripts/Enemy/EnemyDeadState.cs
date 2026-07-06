using UnityEngine;

public class EnemyDeadState : EnemyState
{
    public EnemyDeadState(Enemy enemy) : base(enemy) {}

    public override void EnterState()
    {
        Debug.Log("Enemy is Dead");
    }

    // public override void UpdateState() { }

    public override void ExitState()
    {
        // 적이 죽은 이후 실행시킬 로직
    }

    public override void CheckState()
    {

    }
}
