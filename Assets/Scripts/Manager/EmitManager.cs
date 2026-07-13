using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EmitManager : MonoBehaviour
{
    public static EmitManager Instance { get; private set; }

    [Header("EMIT 가능한 오브젝트들")]
    [SerializeField] public List<IEmit> emitObjects;

    [Header("EMIT 관련 설정")]
    [SerializeField] private float emitPreparationTime; // 선딜(역행하는 오브젝트가 잠시 가만히 멈춰있을 시간)
    [SerializeField] private float emitDurationTime; // EMIT 지속시간
    [SerializeField] private float emitCooldownTime; // 쿨타임
    [SerializeField] public int emitBufferFrames; // EMIT 지속시간동안 역재생할 프레임 수
    [SerializeField] public int emittedFrames;

    [Header("현재 EMIT 상태")]
    [SerializeField] public bool isEmitting;
    public bool isEmitable = true;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        emitBufferFrames = (int) (emitDurationTime * (1/Time.deltaTime));
        emittedFrames = 0;
        emitObjects = new List<IEmit>(emitBufferFrames);
    }

    private void OnEnable() {
        // Event 구독
        EventManager.StartListening("OnEmitPressed", OnEmitPressed);
        EventManager.StartListening("OnEmitAborted", OnEmitAborted);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", OnEmitPressed);
        EventManager.StopListening("OnEmitAborted", OnEmitAborted);
    }

    private void FixedUpdate()
    {
        if (isEmitting)
        {
            // 프레임을 모두 비워내기
            if (emittedFrames < emitBufferFrames)
            {                
                foreach (IEmit obj in emitObjects)
                {
                    obj.EmitFrame();
                }
                emittedFrames++;
            }
            else
            {
                OnEmitFinished();
            }
        }
        else
        {
            if (emitObjects != null)
            {                
                foreach (IEmit obj in emitObjects)
                {
                    obj.RecordFrame();
                }   
            }
        }
    }

    private void OnEmitPressed()
    {
        isEmitable = false;
        Debug.Log("Emit Started");
        emittedFrames = 0;
        foreach (IEmit obj in emitObjects)
        {
            obj.BeginEmit();
        }
        isEmitting = true;
        TimeManager.Instance.StartSlowMotion();
    }

    private void OnEmitFinished()
    {
        Debug.Log("Emit Finished");
        foreach (IEmit obj in emitObjects)
        {
            obj.EndEmit();
        }
        emittedFrames = 0;
        isEmitting = false;
        TimeManager.Instance.StopSlowMotion();
        EventManager.TriggerEvent("OnEmitFinished");
        StartCoroutine(emitCoolDownRoutine());
    }

    private void OnEmitAborted()
    {
        Debug.Log("Emit Aborted");
        // 공격에 의해 Emit이 종료된 상황
        foreach (IEmit obj in emitObjects)
        {
            obj.EndEmit();
        }
        emittedFrames = 0;
        isEmitting = false;
        TimeManager.Instance.InstantNormalTime();
        StartCoroutine(emitCoolDownRoutine());
    }

    private IEnumerator emitCoolDownRoutine()
    {
        yield return new WaitForSeconds(emitCooldownTime);
        isEmitable = true;
    }
}
