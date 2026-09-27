/*
 * 보스의 공격 전략패턴 상태머신
 */

public interface IBossAttackStrategy
{
    bool CanExecute(EnemyBase boss);

    // 패턴 시작/진행/종료
    void Enter(EnemyBase boss);
    void Update(EnemyBase boss);
    void Exit(EnemyBase boss);

    bool IsFinished { get; }
}