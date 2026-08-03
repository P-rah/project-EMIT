using UnityEngine;

public class EnemyEmitController : MonoBehaviour, IEmit
{
    public struct KeyframeStatus
    {
        public Vector2 position;
        public float rotation;
        public Vector2 velocity;
        public EnemyState enemyState;
        public int power;
        public Sprite sprite;
        public bool isFacingLeft;
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
        frame.enemyState = _enemy.currentState;
        frame.power = _enemy.currentPower;
        frame.sprite = spriteRenderer.sprite;
        frame.isFacingLeft = _enemy.isFacingLeft;
        return frame;
    }

    void IEmit.RecordFrame()
    {
        KeyframeStatus keyframe = GetKeyFrame();
        buffer.Insert(keyframe);
    }

    void IEmit.BeginEmit()
    {
        _enemy.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        _enemy.isFacingLeft = false;
        _enemy.ChangeState(_enemy.IdleState, true);
        _enemyAnimator.stopAnimControll();
        _enemy.RootMovement();
        _enemy.outlineController.setEmitOutline();
    }

    void IEmit.EmitFrame()
    {
        if (buffer.Pop(out KeyframeStatus frame))
        {
            // _rb.linearVelocity = -frame.velocity;
            _enemy.SetState(frame.enemyState, -frame.power);
            _rb.MovePosition(frame.position);
            _rb.MoveRotation(frame.rotation);
            
            spriteRenderer.sprite = frame.sprite;
            spriteRenderer.flipX = frame.isFacingLeft;
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
        _enemyAnimator.startAnimControll();
        _enemy.UnrootMovement();
        _enemy.outlineController.setDefaultOutline();
    }

    void IEmit.ResetFrame()
    {
        buffer.Clear();
    }
}
