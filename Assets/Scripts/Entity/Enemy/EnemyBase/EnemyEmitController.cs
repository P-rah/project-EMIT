using EMIT.Utilities;
using System;
using System.Collections;
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
    protected T emitableTarget;
    private Rigidbody2D _rb;
    private SpriteRenderer spriteRenderer;
    private EffectManager effectManager;

    private EmitTrackingEffect currentControllEmitGhost;
    private float ghostDurationTime = 0.3f;
    private EmitTrackingEffect[] emitGhosts;
    private Coroutine ghostRoutine;
    private int currentGhostIndex = 0;

    private Animator _animator;

    private void Awake()
    {
        emitableTarget = GetComponent<T>();
        _rb = emitableTarget.GetComponent<Rigidbody2D>();
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        spriteRenderer = emitableTarget.GetComponent<SpriteRenderer>();
        _animator = emitableTarget.GetComponent<Animator>();
        emitGhosts = new EmitTrackingEffect[10];
    } 
    
    private void Start()
    {
        buffer = new EmitBuffer<KeyframeStatus>(EmitManager.Instance.emitBufferFrames);
        
        EmitManager.Instance.emitObjects.Add(this);
        effectManager = EffectManager.Instance;
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

    public virtual void RecordFrame()
    {
        KeyframeStatus keyframe = GetKeyFrame();
        buffer.Insert(keyframe);
    }

    public virtual void BeginEmit()
    {
        spriteRenderer.material.SetFloat("_OutlineThick", 1f);
        emitableTarget.SetFacingDirection(Vector2.right);
        emitableTarget.RootMovement();
        if (_animator != null) _animator.enabled = false;

        if (emitableTarget is EnemyBase enemyBase)
        {
            enemyBase.ResetInvincibility();
        }

        currentGhostIndex = 0;
    }

    public virtual void BeginGhost()
    {
        ghostRoutine = StartCoroutine(StartEmitGhost());
    }

    public virtual void EndGhost()
    {
        foreach(EmitTrackingEffect ghosts in emitGhosts)
        {
            if(ghosts != null)
                effectManager.ReturnEmitGhost(ghosts);
        }
        if(ghostRoutine != null )StopCoroutine(ghostRoutine);
    }

    public virtual void EmitFrame()
    {
        if (buffer.Pop(out KeyframeStatus frame))
        {
            _rb.MovePosition(frame.position);
            _rb.MoveRotation(frame.rotation);
            emitableTarget.SetPower(-frame.power);
            spriteRenderer.sprite = frame.sprite;
            spriteRenderer.flipX = Direction2D.IsOpposite(frame.facingDir, Vector2.right);
        }
        if (emitGhosts[currentGhostIndex] != null)
        {
            if (emitGhosts[currentGhostIndex].getPos() == frame.position)
            {
                if(emitGhosts[currentGhostIndex].isEndMove())
                    effectManager.ReturnEmitGhost(emitGhosts[currentGhostIndex++]);
            }
        }
    }

    public virtual void EndEmit()
    {
        spriteRenderer.material.SetFloat("_OutlineThick", 0f);
        emitableTarget.Flip();
        spriteRenderer.flipX = false;
        emitableTarget.UnrootMovement();
        if (_animator != null) _animator.enabled = true;
        emitableTarget.ResetPower();
    }

    public virtual void ResetFrame()
    {
        buffer.Clear();
    }

    private IEnumerator StartEmitGhost()
    {
        float ghostTimer = 0f;
        int index = 0;
        currentControllEmitGhost = effectManager.EnableEmitGhost();
        emitGhosts[index++] = currentControllEmitGhost;
        for (int i = 0; i < buffer.Count; i++)
        {
            if(ghostTimer >= ghostDurationTime)
            {
                currentControllEmitGhost.EndMove();
                currentControllEmitGhost = effectManager.EnableEmitGhost();
                emitGhosts[index++] = currentControllEmitGhost;
                ghostTimer = 0f;
            }
            KeyframeStatus frame = buffer.Peek(i);
            currentControllEmitGhost.setGhost(frame.sprite, frame.position, frame.facingDir);

            ghostTimer += 0.01f;
            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
