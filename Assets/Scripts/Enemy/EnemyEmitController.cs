using EMIT.Utilities;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyEmitController : MonoBehaviour, IEmit
{
    /* INTEGRATION:
     * 역행 프레임에는 StateMachine이 소유한 EnemyState 인스턴스와 FacingDir를 그대로 기록한다.
     * integration/enemy-state-merge의 enemyState/currentState 및 isFacingLeft 기반 버퍼로 되돌리면
     * 상태 소유권이 다시 Enemy에 분산되고 회전과 SpriteRenderer.flipX가 서로 다른 기준을 사용하게 된다.
     * 통합 시 state/facingDir 필드와 CurrentState/FacingDir 접근은 유지하고, 해당 브랜치의 외곽선 전환만
     * BeginEmit의 RootMovement 전과 EndEmit의 UnrootMovement 후에 각각 추가한다.
     */
    public struct KeyframeStatus
    {
        public Vector2 position;
        public float rotation;
        public Vector2 velocity;
        public EnemyState state;
        public int power;
        public Sprite sprite;
        public Vector2 facingDir;
    }
    private EmitBuffer<KeyframeStatus> buffer;
    private Enemy _enemy;
    private Rigidbody2D _rb;
    private SpriteRenderer spriteRenderer;
    private EnemyAnimator _enemyAnimator;

    [SerializeField] private Vector3 spawnPosition;

    private void Awake()
    {
        /*
        if (spawnPosition == null)
        {
            spawnPosition = transform.position;
        }
        */
        _enemy = GetComponent<Enemy>();
        _rb = _enemy.GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 더 부드러운 이펙트를 위해 필요함
        spriteRenderer = _enemy.GetComponent<SpriteRenderer>();
        _enemyAnimator = _enemy.GetComponent<EnemyAnimator>();
    } 
    // 버퍼 동적할당
    private void Start()
    {
        buffer = new EmitBuffer<KeyframeStatus>(EmitManager.Instance.emitBufferFrames);
        // EmitManager의 IEmit 배열에 자기 자신을 등록 
        EmitManager.Instance.emitObjects.Add(this);
    }

    private KeyframeStatus GetKeyFrame()
    {
        KeyframeStatus frame;
        frame.position = _rb.position;
        frame.rotation = _rb.rotation;
        frame.velocity = _rb.linearVelocity;
        frame.state = _enemy.CurrentState;
        frame.power = _enemy.currentPower;
        frame.sprite = spriteRenderer.sprite;
        frame.facingDir = _enemy.FacingDir;
        return frame;
    }

    void IEmit.RecordFrame()
    {
        KeyframeStatus keyframe = GetKeyFrame();
        buffer.Insert(keyframe);
    }

    void IEmit.BeginEmit()
    {
        /* INTEGRATION:
         * 역행 시작 시 IdleState로 강제 전환하지 않는 이유는 버퍼에 저장된 실제 상태를 SetState로 재생하기
         * 위해서다. 강제 전환은 ExitState/EnterState 부작용과 currentPower 초기화를 먼저 발생시킬 수 있다.
         * 애니메이터 API는 이 브랜치의 StopAnimControl/StartAnimControl 이름을 유지하고 null을 허용한다.
         */
        _enemy.SetFacingDirection(Vector2.right);
        // _enemy.ChangeState(_enemy.IdleState, true);
        if (_enemyAnimator != null)
        {
            _enemyAnimator.StopAnimControl();
        }
        _enemy.RootMovement();
    }

    void IEmit.EmitFrame()
    {
        if (buffer.Pop(out KeyframeStatus frame))
        {
            // _rb.linearVelocity = -frame.velocity;
            _enemy.SetState(frame.state, -frame.power);
            _rb.MovePosition(frame.position);
            _rb.MoveRotation(frame.rotation);
            
            spriteRenderer.sprite = frame.sprite;
            spriteRenderer.flipX = Direction2D.IsOpposite(frame.facingDir, Vector2.right);
            // _rb.MoveRotation(frame.transform.rotation);
        }
        else
        {
            
        }
    }

    void IEmit.EndEmit()
    {
        _enemy.Flip();
        spriteRenderer.flipX = false;
        if (_enemyAnimator != null)
        {
            _enemyAnimator.StartAnimControl(); 
        }
        _enemy.UnrootMovement();
    }

    void IEmit.ResetFrame()
    {
        buffer.Clear();
    }
}
