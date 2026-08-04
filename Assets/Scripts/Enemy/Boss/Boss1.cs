using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Boss1 : MonoBehaviour, IDamageable
{
    public BossStateMachine StateMachine { get; private set; }
    public BossState CurrentState => StateMachine?.CurrentState;
    public Rigidbody2D Rigidbody { get; private set; }

    [Header("Boss 설정")]
    [SerializeField] private Transform target;
    [SerializeField] private Laser laserPrefab;
    [SerializeField] private List<Transform> hoverPoints;
    [SerializeField] private int bossMaxHealth = 50;
    [SerializeField] private float groggyGravityScale = 1f;
    [SerializeField] private float groggyGrabSpeed = 20f;

    public int CurrentHealth { get; private set; }

    private SpriteRenderer spriteRenderer;
    private BossEmitController emitController;
    private Collider2D[] bossColliders;
    private readonly List<Collider2D> ignoredPlatformColliders = new List<Collider2D>();
    private Color defaultColor;
    private Coroutine grabRoutine;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        Rigidbody.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = spriteRenderer.color;
        bossColliders = GetComponents<Collider2D>();
        emitController = GetComponent<BossEmitController>();

        CurrentHealth = bossMaxHealth;
        StateMachine = new BossStateMachine(this);
    }

    private void Start()
    {
        if (target == null)
        {
            target = Player.InstanceTransform;
        }
        IgnorePlayerCollision();
        StateMachine.ChangeState(StateMachine.MoveState);
    }

    private void Update()
    {
        if (emitController != null && emitController.canEmit
            && EmitManager.Instance != null && EmitManager.Instance.isEmitting)
        {
            return;
        }

        StateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        if (emitController != null && emitController.canEmit
            && EmitManager.Instance != null && EmitManager.Instance.isEmitting)
        {
            return;
        }

        StateMachine.Move();
    }

    public void ReceiveAttack(AttackInfo attackInfo)
    {
        bool isRecoil = StateMachine.IsCurrentState(StateMachine.RecoilState);
        bool isGroggy = StateMachine.IsCurrentState(StateMachine.GroggyState);

        if (!isRecoil && !isGroggy)
        {
            return;
        }

        CurrentHealth -= attackInfo.baseDamage;

        if (CurrentHealth <= 0)
        {
            EventManager.TriggerEvent("OnEmitAborted");
            Destroy(gameObject);
            return;
        }

        if (EmitManager.Instance.isEmitting)
        {
            EventManager.TriggerEvent("OnEmitAborted");
        }

        if (isRecoil)
        {
            StateMachine.ChangeState(StateMachine.GroggyState, true);
        }

        if (!attackInfo.attackIsGrab)
        {
            Rigidbody.AddForce(attackInfo.knockback, ForceMode2D.Impulse);
        }
    }

    public void ReceiveGrab()
    {
        if (!StateMachine.IsCurrentState(StateMachine.GroggyState) || target == null)
        {
            return;
        }

        if (grabRoutine != null)
        {
            StopCoroutine(grabRoutine);
        }

        grabRoutine = StartCoroutine(GrabRoutine());
    }

    public bool TryGetRandomHoverPosition(out Vector2 hoverPosition)
    {
        if (hoverPoints == null || hoverPoints.Count == 0)
        {
            Debug.LogError("Boss1에 Hover Point가 연결되지 않았습니다.", this);
            hoverPosition = Rigidbody.position;
            return false;
        }

        Transform hoverPoint = hoverPoints[Random.Range(0, hoverPoints.Count)];
        if (hoverPoint == null)
        {
            Debug.LogError("Boss1의 Hover Point 목록에 비어 있는 항목이 있습니다.", this);
            hoverPosition = Rigidbody.position;
            return false;
        }

        hoverPosition = hoverPoint.position;
        return true;
    }

    public void PrepareHovering()
    {
        Rigidbody.gravityScale = 0f;
        Rigidbody.linearVelocity = Vector2.zero;
    }

    public void FireLaser()
    {
        if (laserPrefab == null)
        {
            Debug.LogError("Boss1에 Laser Prefab이 연결되지 않았습니다.", this);
            return;
        }

        Instantiate(laserPrefab, Rigidbody.position, Quaternion.identity);
    }

    public void SetRecoilColor()
    {
        spriteRenderer.color = Color.yellow;
    }

    public void ResetColor()
    {
        spriteRenderer.color = defaultColor;
    }

    public void EnterGroggy()
    {
        Rigidbody.gravityScale = groggyGravityScale;
        IgnorePlatformEffectorCollisions();
        
        if (emitController != null)
        {
            emitController.SetCanEmit(true);
        }
        spriteRenderer.color = Color.green;
    }

    public void ExitGroggy()
    {
        RestorePlatformEffectorCollisions();
        PrepareHovering();

        if (emitController != null)
        {
            emitController.SetCanEmit(false);
        }
    }

    public void SetState(BossState nextState)
    {
        StateMachine.SetState(nextState);
    }

    private IEnumerator GrabRoutine()
    {
        float offsetX = target.position.x > transform.position.x ? -0.5f : 0.5f;
        Vector2 targetPosition = new Vector2(target.position.x + offsetX, Rigidbody.position.y);

        Rigidbody.linearVelocity = Vector2.zero;

        while (Vector2.Distance(Rigidbody.position, targetPosition) > 0.1f)
        {
            if (!StateMachine.IsCurrentState(StateMachine.GroggyState))
            {
                grabRoutine = null;
                yield break;
            }

            Vector2 nextPosition = Vector2.MoveTowards(
                Rigidbody.position,
                targetPosition,
                groggyGrabSpeed * Time.fixedDeltaTime
            );

            Rigidbody.MovePosition(nextPosition);
            yield return new WaitForFixedUpdate();
        }

        Rigidbody.MovePosition(targetPosition);
        Rigidbody.linearVelocity = Vector2.zero;
        grabRoutine = null;
    }

    private void IgnorePlatformEffectorCollisions()
    {
        RestorePlatformEffectorCollisions();

        PlatformEffector2D[] effectors = FindObjectsByType<PlatformEffector2D>();
        foreach (PlatformEffector2D effector in effectors)
        {
            Collider2D[] platformColliders = effector.GetComponents<Collider2D>();
            foreach (Collider2D platformCollider in platformColliders)
            {
                foreach (Collider2D bossCollider in bossColliders)
                {
                    Physics2D.IgnoreCollision(bossCollider, platformCollider, true);
                }

                ignoredPlatformColliders.Add(platformCollider);
            }
        }
    }

    private void IgnorePlayerCollision()
    {
        Collider2D playerCollider = target.GetComponent<Collider2D>();
        foreach (Collider2D bossCollider in bossColliders)
        {
            Physics2D.IgnoreCollision(bossCollider, playerCollider, true);
        }
    }

    private void RestorePlatformEffectorCollisions()
    {
        foreach (Collider2D platformCollider in ignoredPlatformColliders)
        {
            if (platformCollider == null)
            {
                continue;
            }

            foreach (Collider2D bossCollider in bossColliders)
            {
                if (bossCollider != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, platformCollider, false);
                }
            }
        }

        ignoredPlatformColliders.Clear();
    }

    private void OnDestroy()
    {
        RestorePlatformEffectorCollisions();
    }
}
