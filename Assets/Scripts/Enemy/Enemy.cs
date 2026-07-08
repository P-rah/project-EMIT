using System.Data.Common;
using JetBrains.Annotations;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

public class Enemy : MonoBehaviour, Attackable
{
    private EnemyState currentState;

    public Transform target; //플레이어의 transform

    public EnemyIdleState IdleState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyDeadState DeadState { get; private set; }
    public EnemyKnockbackState KnockbackState { get; private set;}  //넉백(밀치기) 상태
    public EnemyGrappedState GrappedState { get; private set; }     //넉백이랑 당겨오는 상태랑 분리

    [Header("적 데이터")]
    [SerializeField] private EnemyData data;

    public Rigidbody2D rb;
    public int maxHealth;
    public int baseAttack;
    public float baseSpeed;
    public float jumpHeight;
    public float visionRange;

    public float attackRange;
    public float startAttackRange;
    public float startUpTime;
    public float recoveryTime;

    public float knockbackRate;
    public float knockbackTime;
    public float grapForce;

    [Header("적 상태")]
    public bool canChangeState; // 다음 프레임에서 State가 자동으로 변경 가능한지만 관여
    public int currentHealth;
    public bool isRooted;
    public Vector2 currentKnockBackForce;
    public bool isCurrentHitGrap;

    public Vector2 front; // 적이 바라보는 방향 (넉백 관련 처리 시 필요)

    public virtual void Awake() {

        Debug.Log("Enemy Awake");
        Init();
    }

    public virtual void Start()
    {
        Debug.Log("Enemy is Ready");
    }

    public virtual void Update()
    {
        if (currentState != DeadState) // race condition 방지
        {        
            currentState.UpdateState();
        }
        front = (rb.transform.position.x > target.transform.position.x) ? Vector2.left : Vector2.right;
    }

    public virtual void Init()
    {
        target = Player.InstanceTransform; //플레이어의 싱글톤 Transform 받아옴

        IdleState = new EnemyIdleState(this);
        AttackState = new EnemyAttackState(this);
        ChaseState = new EnemyChaseState(this);
        DeadState =  new EnemyDeadState(this);
        KnockbackState = new EnemyKnockbackState(this);
        GrappedState = new EnemyGrappedState(this);

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        maxHealth = currentHealth = data.baseHealth;
        baseAttack = data.baseAttack;
        baseSpeed = data.baseSpeed;
        jumpHeight = data.jumpHeight;
        visionRange = data.visionRange;
        attackRange = data.attackRange;
        startAttackRange = data.startAttackRange;
        startUpTime = data.startUpTime;
        recoveryTime = data.recoveryTime;
        knockbackRate = data.knockbackRate;
        knockbackTime = data.knockbackTime;
        grapForce = data.grapForce;

        canChangeState = true;
        isRooted = false;

        ChangeState(IdleState);
    }

    public void ChangeState(EnemyState nextState)
    {
        if ( currentState == nextState || !canChangeState ) return;
        currentState?.ExitState();
        currentState = nextState;
        currentState.EnterState();
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        
        if (currentState == AttackState)
        {
            Vector3 dir = new Vector3(target.transform.position.x - transform.position.x, 0, 0).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + dir * attackRange);
        }
        
    }

    public void FixedUpdate()
    {   
        if (isRooted) return;

        currentState?.Move();
    }

    public void ReceiveAttack(AttackInfo attackInfo)
    {
        Vector2 HitPoint = attackInfo.HitPoint;
        currentKnockBackForce = attackInfo.KnockbackForce;
        isCurrentHitGrap = attackInfo.AttackIsGrap;

        if(AttackState.CanBeCancelled())
        {
            AttackState.ExitState();
        }
        // FIX: 임시로 struct 만들어둠
        // 데미지를 받고
        //ReceiveDamage(atk);

        if (currentHealth <= 0)
        {
            ChangeState(DeadState);
        }
        ReceiveKnockback();
    }

    public void ReceiveDamage()
    {
        /*

        역행 중에 밀치거나 당기기가 가능하려면 어떻게 해야 하는가?
        적을 밀친 상태로 역행을 걸었다고 가정해보자.
        역행이 시작되면 적은 플레이어 쪽으로 당겨져오는 상태가 되어야 한다.
        이때 transform을 단순히 조정하면 밀치기/당기기와 함께 사용 불가능.

        해법1: 각 timestamp에서의 velocity를 뒤집어서 가해주기.
        이 상태에서 플레이어 역행을 집어넣으면 velocity가 합산
        threshold 기반으로 데미지 여부 판정
        단점: rb.linearVelocity 등은 물리 프레임에서 연산해야 하는데 데미지는 그렇지 않음.  
        
        해법2: 각 timestamp에서 State도 기록하기.
        단점: State의 
        */

        //int damage = atk.damage;
        //if (currentState == KnockbackState)
        //{
        //    damage *= 2; // FIX: HARDCODED
        //}
        // 평타 = 1
        // 넉백 + 반대방향 평타 = 2
        // 역행 + 반대방향 평타 = 4
        // 역행 + 역행방향 넉백 + 반대방향 평타 = 즉사
        
    }

    public void ReceiveKnockback()
    {
        if (!isCurrentHitGrap) ChangeState(KnockbackState);
        else ChangeState(GrappedState);
    }

    public bool isPlayerInVision()
    {
        return Physics2D.CircleCast(transform.position, visionRange, Vector2.right, 0, LayerMask.GetMask("Player"));
    }

    public bool isPlayerInAttackRange()
    {
        return Physics2D.Raycast(transform.position, front, startAttackRange, LayerMask.GetMask("Player"));

    }
    public void RootMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        isRooted = true;
    }
    public void UnrootMovement()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        isRooted = false;
    }

    public bool IsCurrentState(EnemyState state)
    {
        return currentState == state;
    }
}
