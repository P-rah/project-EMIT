using UnityEngine;

public class ObjectEmitController<T> : MonoBehaviour, IEmit where T : MonoBehaviour, IEmitableTarget
{
    public struct KeyframeStatus
    {
        public Vector2 position;
        public float rotation;
        public int power;
    }
    private EmitBuffer<KeyframeStatus> buffer;
    private Rigidbody2D _rb;
    private SpriteRenderer spriteRenderer;

    protected T emitableTarget;

    private void Awake()
    {
        emitableTarget = GetComponent<T>();
        _rb = emitableTarget.GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        spriteRenderer = emitableTarget.GetComponent <SpriteRenderer>();
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
        frame.power = emitableTarget.currentPower;
        return frame;
    }

    void IEmit.RecordFrame()
    {
        KeyframeStatus keyframe = GetKeyFrame();
        buffer.Insert(keyframe);
    }

    void IEmit.BeginEmit()
    {
        spriteRenderer.material.SetFloat("_OutlineThick", 1f);
        emitableTarget.RootMovement();
    }

    void IEmit.EmitFrame()
    {
        if (buffer.Pop(out KeyframeStatus frame))
        {
            _rb.MovePosition(frame.position);
            _rb.MoveRotation(frame.rotation);
            emitableTarget.SetPower(-frame.power);
            // _rb.MoveRotation(frame.transform.rotation);
        }
    }

    void IEmit.EndEmit()
    {
        spriteRenderer.material.SetFloat("_OutlineThick", 0f);
        emitableTarget.UnrootMovement();
        emitableTarget.ResetPower();
    }

    void IEmit.ResetFrame()
    {
        buffer.Clear();
    }

    void IEmit.BeginGhost()
    {
        throw new System.NotImplementedException();
    }

    void IEmit.EndGhost()
    {
        throw new System.NotImplementedException();
    }
}
