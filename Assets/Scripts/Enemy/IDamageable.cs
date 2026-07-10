using UnityEngine;

public struct AttackInfo
{
    public Vector2 knockbackForce;  // 넉백의 크기와 방향
    public bool attackIsGrab;       // 공격이 당기기인지(true) 밀치기인지(false)
}

public interface IDamageable
{
    void ReceiveAttack(AttackInfo Info);
}