using UnityEngine;

public struct AttackInfo
{
    public int baseDamage;
    public int power;               // 부호는 방향, 절댓값은 크기
    public Vector2 knockback;  // 넉백의 크기와 방향
    public bool attackIsGrab;       // 공격이 당기기인지(true) 밀치기인지(false)
}

//대미지를 입을 수 있는 오브젝트
public interface IDamageable
{
    int currentPower { get; }
    int dmgType { get; set; }
    void ReceiveAttack(AttackInfo Info);
}
