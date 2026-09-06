using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using System.Threading;

[RequireComponent(typeof(Rigidbody2D))] 
[RequireComponent(typeof(BoxCollider2D))]
public class MovingPlatform : MonoBehaviour
{
    private enum MoveCondition
    {
        PlayerRides,
        AnythingRides,
        Always
    }

    private enum TraversalMethod
    {
        Loop,
        PingPong
    }

    private Rigidbody2D rb;
    public Vector2 DeltaPosition { get; private set; }
    private float deltaPositionFixedTime = float.NegativeInfinity;
    private TweenerCore<Vector3, Path, PathOptions> pathTween;

    [Tooltip("플랫폼이 시작할 지점")]
    [SerializeField] private Transform initialPos;
    [Tooltip("플랫폼이 경유할 지점들. 마지막 지점이 플랫폼의 도착점")]
    [SerializeField] private Transform[] pathPoints;
    [SerializeField] private float cycleSeconds;
    [Tooltip("플랫폼이 활성화되기까지 걸리는 시간")]
    [SerializeField] private float activationTime = 1f;
    [Tooltip("플랫폼에서 오브젝트가 하차했다고 인지하기까지 필요한 시간")]
    [SerializeField] private float deactivationTime = 2f;
    [Tooltip("플랫폼이 특정 지점에 도달한 후 정지해있을 시간")]
    [SerializeField] private float pauseTime = 1f;
    [Tooltip("플랫폼이 어떤 조건에서만 움직일지")]
    [SerializeField] private MoveCondition moveCondition;
    [Tooltip("플랫폼의 왕복/순환 방식")]
    [SerializeField] private TraversalMethod traversalMethod;
    [Tooltip("플랫폼이 아무것도 태우고 있지 않을 때 왕복할지 여부")]
    [SerializeField] private bool returnWhileNotCarrying;
    [Tooltip("직선/곡선/BezierCubic 중 움직임 보간 방식")]
    [SerializeField] private PathType pathType;

    [SerializeField] private int pathPointIndex;
    [SerializeField] private bool playerIsCarried;
    [SerializeField] private bool anythingIsCarried;
    [SerializeField] private float activationTimer;
    [SerializeField] private float deactivationTimer;
    
    private bool movementStarted;
    private bool returningToStart;
    private Vector2 startPosition;
    private float returnSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (initialPos == null)
        {
            initialPos = transform;
        }

        rb.position = initialPos.position;
        startPosition = rb.position;
        activationTimer = 0f;
        deactivationTimer = 0f;
        pathPointIndex = 0;
        CreatePathTween();
    }

    private void FixedUpdate()
    {
        GetDeltaPositionAndMove();
    }

    private void UpdateTimers()
    {
        if (pathTween == null)
        {
            return;
        }

        if (PlatformCanMove())
        {
            deactivationTimer = 0f;

            if (movementStarted)
            {
                activationTimer = 0f;
                return;
            }

            activationTimer += Time.fixedDeltaTime;

            if (activationTimer >= activationTime)
            {
                StartMovement();
            }
        }
        else
        {
            activationTimer = 0f;

            if (!movementStarted)
            {
                deactivationTimer = 0f;
                return;
            }

            deactivationTimer += Time.fixedDeltaTime;

            if (deactivationTimer >= deactivationTime)
            {
                StopMovement();
            }

        }
    }

    private void StartMovement()
    {
        if (pathTween == null)
        {
            return;
        }

        activationTimer = 0f;
        deactivationTimer = 0f;
        movementStarted = true;

        if (returningToStart)
        {
            return;
        }

        // 하차로 복귀 중이었다면 정방향으로 재출발한다.
        // 단순 일시정지였으면 PingPong의 역방향도 그대로 재개한다.
        if (returnWhileNotCarrying || pathTween.Elapsed() == 0f)
        {
            pathTween.PlayForward();
        }
        else
        {
            pathTween.Play();
        }
    }

    private void StopMovement() {
        if (!movementStarted)
        {
            return;
        }

        movementStarted = false;
        activationTimer = 0f;
        deactivationTimer = 0f;
        DeltaPosition = Vector2.zero;

        if (pathTween == null)
        {
            return;
        }

        if (returnWhileNotCarrying && !returningToStart)
        {
            pathTween.PlayBackwards();
            return;
        }

        pathTween.Pause();
    }

    /// <summary> rb.position에서 출발해서, 주어진 Path를 cycleSeconds 동안 따라가고,
    /// PathPlugin이 매 tick 계산한 위치를 PublishDeltaPositionAndMove에게 넘겨라. </summary>
    private void CreatePathTween()
    {
        if (pathPoints == null || pathPoints.Length == 0)
        {
            Debug.LogError("MovingPlatform의 Path Points가 비어 있습니다.", this);
            return;
        }

        if (cycleSeconds <= 0f)
        {
            Debug.LogError("MovingPlatform의 Cycle Seconds는 0보다 커야 합니다.", this);
            return;
        }

        Vector3[] wayPoints = ToPositions(pathPoints);
        if (wayPoints == null)
        {
            return;
        }

        pathTween = DOTween.To(
            PathPlugin.Get(),                       // Path Tween 계산에 사용할 플러그인 가져옴
            () => rb.position,                      // 현재 값을 읽는 Getter
            PublishDeltaPositionAndMove,            // PathPlugin이 계산한 새로운 위치를 전달받는 Setter
            new Path(pathType, wayPoints, 10, null),
            cycleSeconds
        )
        .SetTarget(rb)
        .SetUpdate(UpdateType.Manual)
        .SetAutoKill(false)
        .SetEase(Ease.Linear)
        .Pause();

        pathTween.plugOptions.isRigidbody2D = true;
        pathTween.plugOptions.mode = PathMode.Full3D;
        pathTween.ForceInit();
        returnSpeed = pathTween.PathLength() / cycleSeconds;
    }

    // DOTween을 한 틱 진행시킴
    private void UpdatePathForCurrentPhysicsStep()
    {
        if (returningToStart)
        {
            if (!movementStarted && !returnWhileNotCarrying)
            {
                return;
            }

            Vector2 nextPosition = Vector2.MoveTowards(
                rb.position, startPosition, returnSpeed * Time.fixedDeltaTime);
            PublishDeltaPositionAndMove(nextPosition);

            if (nextPosition == startPosition)
            {
                returningToStart = false;
                // 이미 시작점까지 이동한 뒤에만 Tween을 되감아 위치 점프를 방지한다.
                pathTween.Rewind();
                if (movementStarted)
                {
                    pathTween.PlayForward();
                }
            }
            return;
        }

        if (!pathTween.IsPlaying())
        {
            return;
        }

        pathTween.ManualUpdate(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);

        if (!pathTween.IsBackwards() && pathTween.IsComplete())
        {
            if (traversalMethod == TraversalMethod.Loop)
            {
                returningToStart = true;
            }
            else
            {
                pathTween.PlayBackwards();
            }
        }
        else if (pathTween.IsBackwards() && pathTween.Elapsed() == 0f && movementStarted)
        {
            pathTween.PlayForward();
        }
    }

    private void PublishDeltaPositionAndMove(Vector3 targetPosition)
    {
        Vector2 nextPosition = targetPosition;
        DeltaPosition = nextPosition - rb.position;
        deltaPositionFixedTime = Time.fixedTime;
        rb.MovePosition(nextPosition);
    }
    private void OnDestroy()
    {
        pathTween?.Kill();
    }
    /// <summary> 지난 물리 프레임 대비 이동량을 계산함 </summary>
    public Vector2 GetDeltaPositionAndMove()
    {
        if (!isActiveAndEnabled || pathTween == null)
        {
            return Vector2.zero;
        }

        if (deltaPositionFixedTime != Time.fixedTime)
        {
            DeltaPosition = Vector2.zero;
            deltaPositionFixedTime = Time.fixedTime;

            // 플레이어가 먼저 호출해도 타이머와 이동을 같은 틱에 한 번만 갱신한다.
            UpdateTimers();
            UpdatePathForCurrentPhysicsStep();
        }

        return DeltaPosition;
    }
    private Vector3[] ToPositions(Transform[] pathPoints)
    {
        Vector3[] wayPoints = new Vector3[pathPoints.Length];

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null)
            {
                Debug.LogError($"MovingPlatform의 Path Points[{i}]가 비어 있습니다.", this);
                return null;
            }
            wayPoints[i] = pathPoints[i].position;
        }
        return wayPoints;
    }

    private bool PlatformCanMove()
    {
        switch (moveCondition)
        {
            case MoveCondition.Always:
                return true;

            case MoveCondition.AnythingRides:
                return playerIsCarried || anythingIsCarried;

            case MoveCondition.PlayerRides:
                return playerIsCarried;

            default:
                return false;
        }
    }

    private bool IsFeet(Collider2D other)
    {
        return other is CircleCollider2D && other.isTrigger &&
            other.name == "GroundCheck" && other.attachedRigidbody != null &&
            other.attachedRigidbody.gameObject.layer == 3;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsFeet(other))
        {
            playerIsCarried = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsFeet(other))
        {
            playerIsCarried = false;
        }
    }

    // 플레이어 몸체 충돌은 제외하고, 다른 물체의 기존 판정은 유지한다.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != 3)
        {
            anythingIsCarried = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer != 3)
        {
            anythingIsCarried = false;
        }
    }
}
