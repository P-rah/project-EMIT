using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyEmitController : MonoBehaviour, IEmit
{
    public struct KeyframeStatus
    {
        public Vector2 position;
        public float rotation;
        public EnemyState enemyState;
    }
    private EmitBuffer<KeyframeStatus> buffer;
    private Enemy _enemy;
    private Rigidbody2D _rb;
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
        frame.enemyState = _enemy.currentState;
        return frame;
    }

    void IEmit.RecordFrame()
    {
        KeyframeStatus keyframe = GetKeyFrame();
        buffer.Insert(keyframe);
    }

    void IEmit.BeginEmit()
    {
        _enemy.RootMovement();
    }

    void IEmit.EmitFrame()
    {
        if (buffer.Pop(out KeyframeStatus frame))
        {
            _rb.MovePosition(frame.position);
            _rb.MoveRotation(frame.rotation);
            // _rb.MoveRotation(frame.transform.rotation);
        }
        else
        {
            
        }
    }

    void IEmit.EndEmit()
    {
        _enemy.UnrootMovement();
    }
}
