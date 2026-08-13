using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using System.Threading;

// 미완성임

[RequireComponent(typeof(Rigidbody2D))] 
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
    
    private CancellationTokenSource transitionCts;
    private CancellationTokenSource movementCts;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (initialPos == null)
        {
            initialPos = transform;
        }
        activationTimer = 0f;
        pathPointIndex = 0;
    }

    private void Start()
    {
        rb.position = initialPos.position;
        // StartMovement();   
    }

    private void FixedUpdate()
    {
        UpdateActivationTimer();
        GetDeltaPositionAndMove();
    }

    private void UpdateActivationTimer()
    {
        if (!PlatformCanMove())
        {
            activationTimer = 0f;
            return;
        }

        activationTimer += Time.fixedDeltaTime;

        if (activationTimer >= activationTime)
        {
            BeginMovement();
        }
    }

    private void UpdateDeactivationTimer()
    {
        if (PlatformCanMove())
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

    private void BeginMovement()
    {
        Vector2[] wayPoints = new Vector2[pathPoints.Length];
        for (int i = 0; i < wayPoints.Length; i++)
        {
            wayPoints[i] = pathPoints[i].position;
        }
        if (pathPointIndex < wayPoints.Length - 1)
        {
            // Index 범위 검사 완료
            Vector2 nextPoint = wayPoints[pathPointIndex++];
            
        }
    }

    // Start ->> StartMovement
    private void StartMovement()
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

        Vector2[] wayPoints = new Vector2[pathPoints.Length];

        for (int i = 0; i < pathPoints.Length; i++)
        {
            if (pathPoints[i] == null)
            {
                continue;
            }
            wayPoints[i] = pathPoints[i].position;
        }

        StartDeltaPositionTrackingPath(wayPoints);
    }

    private void StopMovement() {
        if (returnWhileNotCarrying)
        {
            // 왕복이라면 역순으로 감아서 initialPos로
            // cycle이라면 index 확인 후 가까운 방향으로 initialPos로
        }
        // 그냥 정지
    }

    
    private void OnDestroy()
    {
        pathTween?.Kill();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            playerIsCarried = true;   
        }
        anythingIsCarried = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            playerIsCarried = false;   
        }
        anythingIsCarried = false;
    }

    private bool PlatformCanMove()
    {
        switch (moveCondition)
        {
            case MoveCondition.Always:
                return true;

            case MoveCondition.AnythingRides:
                return anythingIsCarried;
            
            case MoveCondition.PlayerRides:
                return playerIsCarried;

            default:
                return false;
        }
    }

    /* DeltaPosition 관련 */

    /// <summary>
    /// 지난 물리 프레임 대비 이동량을 계산함  
    /// </summary>
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
            
            UpdatePathForCurrentPhysicsStep();
        }

        return DeltaPosition;
    }

    // StartMovement -> StartDeltaPositionTrackingPath
    /// <summary>
    /// rb.position에서 출발해서, 주어진 Path를 cycleSeconds 동안 따라가고, 
    /// PathPlugin이 매 tick 계산한 위치를 PublishDeltaPositionAndMove에게 넘겨라.
    /// </summary>
    private void StartDeltaPositionTrackingPath(Vector2[] wayPoints)
    {
        Vector3[] path = new Vector3[wayPoints.Length];
        for (int i = 0; i < wayPoints.Length; i++)
        {
            path[i] = wayPoints[i];
        }

        pathTween = DOTween.To(
            PathPlugin.Get(),                       // Path Tween 계산에 사용할 플러그인 가져옴
            () => rb.position,                      // 현재 값을 읽는 Getter
            PublishDeltaPositionAndMove,            // PathPlugin이 계산한 새로운 위치를 전달받는 Setter
            new Path(pathType, path, 10, null),
            cycleSeconds
        )
        .SetTarget(rb)
        .SetUpdate(UpdateType.Fixed);

        pathTween.plugOptions.isRigidbody2D = true;
        pathTween.plugOptions.mode = PathMode.Full3D;
        pathTween.SetEase(Ease.Linear);
    }

    // DOTween을 한 틱 진행시킴
    // GetDeltaPositionAndMove -> UpdatePathForCurrentPhysicsStep
    private void UpdatePathForCurrentPhysicsStep()
    {
        pathTween?.ManualUpdate(
            Time.fixedDeltaTime,
            Time.fixedUnscaledDeltaTime
        );
    }

    private void PublishDeltaPositionAndMove(Vector3 targetPosition)
    {
        // targetPosition은 Plugin으로부터 전달받음
        Vector2 nextPosition = targetPosition;
        DeltaPosition = nextPosition - rb.position;
        deltaPositionFixedTime = Time.fixedTime;
        rb.MovePosition(nextPosition);
    }

}
