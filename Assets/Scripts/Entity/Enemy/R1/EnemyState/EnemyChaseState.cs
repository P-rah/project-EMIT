using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    private const float MaxGroundAngle = 80f;
    private const float GroundProbeDistance = 0.3f;
    private readonly List<ContactPoint2D> groundContacts = new List<ContactPoint2D>(8);
    private readonly List<RaycastHit2D> groundHits = new List<RaycastHit2D>(8);
    private Collider2D bodyCollider;
    private Collider2D groundCollider;
    private ContactFilter2D groundFilter;
    private Rigidbody2D.SlideMovement slideMovement;
    private float initialGravityScale;

    public EnemyChaseState(IEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void EnterState()
    {
        if (enemy.rb == null) return;

        initialGravityScale = enemy.rb.gravityScale;
        groundCollider = null;
        bodyCollider = null;
        foreach (Collider2D collider in enemy.GetComponents<Collider2D>())
        {
            if (!collider.enabled || collider.isTrigger || collider.attachedRigidbody != enemy.rb) continue;
            bodyCollider = collider;
            break;
        }

        groundFilter = new ContactFilter2D { useTriggers = false };
        groundFilter.SetLayerMask(LayerMask.GetMask("Ground", "Platform"));
        slideMovement = new Rigidbody2D.SlideMovement
        {
            maxIterations = 4,
            surfaceSlideAngle = MaxGroundAngle,
            gravitySlipAngle = 90f,
            surfaceUp = Vector2.up,
            surfaceAnchor = Vector2.down * GroundProbeDistance,
            gravity = Vector2.zero,
            selectedCollider = bodyCollider,
            useNoMove = true,
            useSimulationMove = false,
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Ground")
        };
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }

    public override void ExitState()
    {
        // 추적 중 꺼 둔 중력이 공격/넉백/잡기 상태에 남지 않도록 복구한다.
        if (enemy.rb != null) enemy.rb.gravityScale = initialGravityScale;
        groundCollider = null;
    }

    // RigidBody 기반 Movement이므로 Enemy.FixedUpdate()에서 해당 함수 호출됨
    public override void Move()
    {
        if (enemy.rb == null)
        {
            Debug.LogError("Enemy requires RigidBody2D");
            return;
        }
        if (enemy.rb.bodyType != RigidbodyType2D.Dynamic || enemy.isRooted) return;

        float speed = enemy.baseSpeed * enemy.targetDir.x;
        bool wasGrounded = groundCollider != null;
        if (!TryGetGround(out Vector2 normal, out float snapDistance))
        {
            enemy.rb.gravityScale = initialGravityScale;
            // 오르막 이동으로 생긴 Y 속도가 발판 끝에서 점프처럼 남지 않게 한다.
            if (wasGrounded && enemy.rb.linearVelocityY > 0f) enemy.rb.linearVelocityY = 0f;
            enemy.rb.linearVelocityX = speed;
            return;
        }

        enemy.rb.gravityScale = 0f;
        if (Mathf.Approximately(speed, 0f))
        {
            enemy.rb.linearVelocity = Vector2.zero;
            return;
        }

        // 플레이어처럼 수평 방향을 지면 접선 방향으로 바꾸되 전체 보행 속도는 유지한다.
        Vector2 tangent = new Vector2(normal.y, -normal.x).normalized;
        Vector2 velocity = tangent * speed + Vector2.down * (snapDistance / Time.fixedDeltaTime);
        if (IsOneWayPlatform(groundCollider) ||
            (slideMovement.layerMask.value & (1 << groundCollider.gameObject.layer)) == 0)
        {
            // One-way는 유효한 접촉을 확인한 뒤 물리 시뮬레이션으로 이동한다.
            enemy.rb.linearVelocity = velocity;
            return;
        }

        var result = enemy.rb.Slide(velocity, Time.fixedDeltaTime, slideMovement);
        enemy.rb.MovePosition(result.position);
    }

    private bool TryGetGround(out Vector2 normal, out float snapDistance)
    {
        Collider2D previousGround = groundCollider;
        groundCollider = null;
        normal = Vector2.up;
        snapDistance = 0f;
        if (bodyCollider == null || !bodyCollider.enabled) return false;

        float skin = Physics2D.defaultContactOffset * 2f;
        float feetY = bodyCollider.bounds.min.y + skin;
        float minNormalY = Mathf.Cos(MaxGroundAngle * Mathf.Deg2Rad);
        float bestScore = float.PositiveInfinity;
        bodyCollider.GetContacts(groundFilter, groundContacts);
        foreach (ContactPoint2D contact in groundContacts)
        {
            Collider2D surface = contact.collider == bodyCollider ? contact.otherCollider : contact.collider;
            // 적의 BoxCollider 하단 접촉만 사용한다. 몸 옆의 접촉은 착지가 아니다.
            if (!contact.enabled || !CanStandOn(surface) || contact.normal.y < minNormalY ||
                contact.point.y > feetY) continue;

            // 평지와 오르막을 동시에 밟을 때 진행 방향을 막는 오르막을 먼저 따른다.
            float score = Mathf.Approximately(enemy.targetDir.x, 0f)
                ? -contact.normal.y : contact.normal.x * enemy.targetDir.x;
            if (score >= bestScore) continue;
            bestScore = score;
            groundCollider = surface;
            normal = contact.normal.normalized;
        }

        if (groundCollider != null) return true;
        if (previousGround == null) return false;

        // 새 착지는 실제 접촉으로만 인정하고, 캐스트는 기존 보행의 작은 틈만 잇는다.
        float probeDistance = Mathf.Approximately(enemy.baseSpeed * enemy.targetDir.x, 0f)
            ? skin : GroundProbeDistance;
        bodyCollider.Cast(Vector2.down, groundFilter, groundHits, probeDistance, true);
        float closestDistance = float.PositiveInfinity;
        foreach (RaycastHit2D hit in groundHits)
        {
            if (!CanStandOn(hit.collider) || hit.distance <= 0f || hit.distance >= closestDistance ||
                hit.normal.y < minNormalY || hit.point.y > feetY) continue;
            if (IsOneWayPlatform(hit.collider) && hit.collider != previousGround) continue;

            closestDistance = hit.distance;
            groundCollider = hit.collider;
            normal = hit.normal.normalized;
            snapDistance = Mathf.Max(0f, hit.distance - skin);
        }

        return groundCollider != null;
    }

    private bool CanStandOn(Collider2D surface)
    {
        return surface != null && surface.enabled && !surface.isTrigger && surface.attachedRigidbody != enemy.rb &&
            !Physics2D.GetIgnoreLayerCollision(bodyCollider.gameObject.layer, surface.gameObject.layer) &&
            !Physics2D.GetIgnoreCollision(bodyCollider, surface);
    }

    private static bool IsOneWayPlatform(Collider2D surface)
    {
        if (!surface.usedByEffector) return false;
        var effector = surface.GetComponent<PlatformEffector2D>();
        if (effector == null && surface.attachedRigidbody != null)
            effector = surface.attachedRigidbody.GetComponent<PlatformEffector2D>();
        return effector != null && effector.isActiveAndEnabled && effector.useOneWay;
    }
}
