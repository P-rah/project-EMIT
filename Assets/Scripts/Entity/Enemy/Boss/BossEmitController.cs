//using UnityEngine;

//[RequireComponent(typeof(Boss1))]
//public class BossEmitController : MonoBehaviour, IEmit
//{
//    public struct KeyframeStatus
//    {
//        public Vector2 position;
//        public float rotation;
//        public Vector2 velocity;
//        public StateBase<BossStateMachine> state;
//        public Sprite sprite;
//    }

//    [SerializeField] public bool canEmit;

//    private EmitBuffer<KeyframeStatus> buffer;
//    private Boss1 boss;
//    private Rigidbody2D rb;
//    private SpriteRenderer spriteRenderer;
//    private bool isParticipatingInEmit;
//    private RigidbodyType2D bodyTypeBeforeEmit;
//    private float gravityScaleBeforeEmit;
//    private Vector2 lastEmittedVelocity;

//    private void Awake()
//    {
//        boss = GetComponent<Boss1>();
//        rb = GetComponent<Rigidbody2D>();
//        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
//        spriteRenderer = GetComponent<SpriteRenderer>();
//    }

//    private void Start()
//    {
//        if (EmitManager.Instance == null)
//        {
//            Debug.LogError("BossEmitController가 EmitManager를 찾지 못했습니다.", this);
//            return;
//        }

//        buffer = new EmitBuffer<KeyframeStatus>(EmitManager.Instance.emitBufferFrames);
//        EmitManager.Instance.emitObjects.Add(this);
//    }

//    public void SetCanEmit(bool value)
//    {
//        if (canEmit == value)
//        {
//            return;
//        }

//        canEmit = value;
//        buffer?.Clear();
//    }

//    private KeyframeStatus GetKeyFrame()
//    {
//        KeyframeStatus frame;
//        frame.position = rb.position;
//        frame.rotation = rb.rotation;
//        frame.velocity = rb.linearVelocity;
//        frame.state = boss.CurrentState;
//        frame.sprite = spriteRenderer.sprite;
//        return frame;
//    }

//    void IEmit.RecordFrame()
//    {
//        if (!canEmit || buffer == null)
//        {
//            return;
//        }

//        buffer.Insert(GetKeyFrame());
//    }

//    void IEmit.BeginEmit()
//    {
//        if (!canEmit || buffer == null)
//        {
//            return;
//        }

//        isParticipatingInEmit = true;
//        bodyTypeBeforeEmit = rb.bodyType;
//        gravityScaleBeforeEmit = rb.gravityScale;
//        lastEmittedVelocity = rb.linearVelocity;

//        rb.linearVelocity = Vector2.zero;
//        rb.bodyType = RigidbodyType2D.Kinematic;
//        rb.gravityScale = 0f;
//    }

//    void IEmit.EmitFrame()
//    {
//        if (!isParticipatingInEmit)
//        {
//            return;
//        }

//        if (buffer.Pop(out KeyframeStatus frame))
//        {
//            //boss.SetState(frame.state);
//            rb.MovePosition(frame.position);
//            rb.MoveRotation(frame.rotation);
//            lastEmittedVelocity = frame.velocity;
//            spriteRenderer.sprite = frame.sprite;
//        }
//    }

//    void IEmit.EndEmit()
//    {
//        if (!isParticipatingInEmit)
//        {
//            return;
//        }

//        rb.bodyType = bodyTypeBeforeEmit;
//        rb.gravityScale = gravityScaleBeforeEmit;
//        rb.linearVelocity = lastEmittedVelocity;
//        isParticipatingInEmit = false;
//    }

//    void IEmit.ResetFrame()
//    {
//        buffer?.Clear();
//    }

//    private void OnDestroy()
//    {
//        if (EmitManager.Instance != null && EmitManager.Instance.emitObjects != null)
//        {
//            EmitManager.Instance.emitObjects.Remove(this);
//        }
//    }
//}
