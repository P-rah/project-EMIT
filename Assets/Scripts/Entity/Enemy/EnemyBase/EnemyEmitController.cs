using EMIT.Utilities;
using UnityEngine;

public class EnemyEmitController<T> : MonoBehaviour, IEmit where T : MonoBehaviour, IEmitableTarget
{
    public struct KeyframeStatus
    {
        public Vector2 position;
        public float rotation;
        public Vector2 velocity;
        public int power;
        public Sprite sprite;
        public Vector2 facingDir;
    }
    private EmitBuffer<KeyframeStatus> buffer;
    private T emitableTarget;
    private Rigidbody2D _rb;
    private SpriteRenderer spriteRenderer;

    private Animator _animator;

    private void Awake()
    {
        emitableTarget = GetComponent<T>();
        _rb = emitableTarget.GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        spriteRenderer = emitableTarget.GetComponent<SpriteRenderer>();
        _animator = emitableTarget.GetComponent<Animator>();
    } 
    
    private void Start()
    {
        buffer = new EmitBuffer<KeyframeStatus>(EmitManager.Instance.emitBufferFrames);
        
        EmitManager.Instance.emitObjects.Add(this);
    }

    private KeyframeStatus GetKeyFrame()
    {
        KeyframeStatus frame;
        frame.position = _rb.position;
        frame.rotation = _rb.rotation;
        frame.velocity = _rb.linearVelocity;
        frame.power = emitableTarget.currentPower;
        frame.sprite = spriteRenderer.sprite;
        frame.facingDir = emitableTarget.FacingDir;
        return frame;
    }

    void IEmit.RecordFrame()
    {
        KeyframeStatus keyframe = GetKeyFrame();
        buffer.Insert(keyframe);
    }

    void IEmit.BeginEmit()
    {
        emitableTarget.SetFacingDirection(Vector2.right);
        emitableTarget.RootMovement();
        if (_animator != null) _animator.enabled = false;
    }

    void IEmit.EmitFrame()
    {
        if (buffer.Pop(out KeyframeStatus frame))
        {
            emitableTarget.SetPower(-frame.power);
            _rb.MovePosition(frame.position);
            _rb.MoveRotation(frame.rotation);
            
            spriteRenderer.sprite = frame.sprite;
            spriteRenderer.flipX = Direction2D.IsOpposite(frame.facingDir, Vector2.right);
        }
    }

    void IEmit.EndEmit()
    {
        emitableTarget.Flip();
        spriteRenderer.flipX = false;
        emitableTarget.UnrootMovement();
        if (_animator != null) _animator.enabled = true;
    }

    void IEmit.ResetFrame()
    {
        buffer.Clear();
    }
}
