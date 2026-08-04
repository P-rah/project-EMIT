using EMIT.Utilities;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyEmitController : MonoBehaviour, IEmit
{
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
