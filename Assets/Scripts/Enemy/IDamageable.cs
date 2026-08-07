using UnityEngine;

/*
 * 대미지를 받을 수 있는 적의 인터페이스
 */

public struct AttackInfo
{
    public int baseDamage;
    public int power;               // 부호는 방향, 절댓값은 크기
    public Vector2 knockback;  // 넉백의 크기와 방향
    public bool attackIsGrab;       // 공격이 당기기인지(true) 밀치기인지(false)
}

public interface IDamageable
{
    void ReceiveAttack(AttackInfo Info);
}
