using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Scriptable Object/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("설명")]
    public string enemyName;
    public string enemyInfo; 

    [Header("기본 수치")]
    public int baseHealth;
    public int baseAttack;

    public float baseSpeed;
    public float jumpHeight;
    public float visionRange;

    [Header("공격 관련")]
    public float attackRange; // 공격 범위
    public float startAttackRange; // attackRange보다 살짝 작음, 이 범위 안에 들어가야 공격 시작함
    // 선딜 시간 + 후딜 시간 = 공격속도
    public float startUpTime; // 선딜레이 시간
    public float recoveryTime; // 후딜레이 시간

    [Header("피격 관련")]
    public float knockbackRate;
    public float knockbackTime;
    public float grabForce;
    public float HitRecoveryTime;
}
