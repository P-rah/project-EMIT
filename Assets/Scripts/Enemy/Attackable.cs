using UnityEngine;

public struct AttackInfo
{
    public Vector2 HitPoint;        // 타격된 위치 (파티클용)
    public Vector2 KnockbackForce;  // 넉백의 크기와 방향
    public bool AttackIsGrap;       // 공격이 당기기인지(true) 밀치기인지(false)
}

public interface Attackable
{
    void ReceiveAttack(AttackInfo Info);
}