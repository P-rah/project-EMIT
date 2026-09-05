using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitManager : MonoBehaviour
{
    public static EmitManager Instance { get; private set; }

    [Header("EMIT 가능한 오브젝트들")]
    [SerializeField] public List<IEmit> emitObjects;

    [Header("EMIT 관련 설정")]
    [SerializeField] private float emitPreparationTime; // 선딜(역행하는 오브젝트가 잠시 가만히 멈춰있을 시간)
    [SerializeField] private float emitDurationTime; // EMIT 지속시간
    [SerializeField] private float emitCoolTime;    //EMIT 회복 쿨타임
    [SerializeField] public int emitBufferFrames; // EMIT 지속시간동안 역재생할 프레임 수
    [SerializeField] public int emittedFrames;

    [Header("현재 EMIT 상태")]
    [SerializeField] public bool isEmitting;
    public bool isEmittable = true;             //역행 가능 상태
    [SerializeField] private bool isEmitCanRecovery = true; //역행 가능 상태로 전환 가능한가 여부
    private float currentEmitableTime;

    [Header("EMIT 효과음")]
    public AudioClip emitStartSound1;
    public AudioClip emitStartSound2;
    public AudioClip emitEndSound;
    public AudioClip emitClockSound1;
    public AudioClip emitClockSound2;

    private Coroutine startEmitTimer;

    //[Header("UI 컨트롤러")]
    private PlayerUIController playerUIController;

    public void ConnectPlayerUIController(PlayerUIController playerUI)
    {
        playerUIController = playerUI;
        currentEmitableTime = emitDurationTime;
        updateEmitUI();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        emitBufferFrames = (int)(emitDurationTime * (1 / Time.fixedDeltaTime));
        emittedFrames = 0;
        emitObjects = new List<IEmit>(emitBufferFrames);
        currentEmitableTime = emitDurationTime;
    }

    private void OnEnable() {
        // Event 구독
        EventManager.StartListening("OnEmitPressed", OnEmitPressed);
        EventManager.StartListening("StopEmit", EmitReset);
        EventManager.StartListening("OnEmitEnd", OnEmitFinished);
        EventManager.StartListening("OnPlayerDieEmit", OnPlayerDieEmit);    //플레이어 사망 시 역행 비주얼용
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", OnEmitPressed);
        EventManager.StopListening("StopEmit", EmitReset);
        EventManager.StopListening("OnEmitEnd", OnEmitFinished);
        EventManager.StopListening("OnPlayerDieEmit", OnPlayerDieEmit);
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
        if (!isEmittable) return;
        isEmittable = false;
        Debug.Log("Emit Started");
        emittedFrames = 0;
        foreach (IEmit obj in emitObjects)
        {
            obj.BeginGhost();
        }
        isEmitting = true;
        TimeManager.Instance.TimeStop();
        SoundManager.Instance.PlaySFX(emitStartSound1);
        SoundManager.Instance.PlaySFX(emitStartSound2);
        SoundManager.Instance.StartEmitBGM();
        EventManager.TriggerEvent("OnEmitStart");
        StartCoroutine(StartEmitPrepareTimer());
    }

    private void StartEmit()
    {
        foreach (IEmit obj in emitObjects)
        {
            obj.EndGhost();
            obj.BeginEmit();
        }
        startEmitTimer = StartCoroutine(StartEmitTimer());
        TimeManager.Instance.TimeStart();
        StartCoroutine(emitClockSound());
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
        if (startEmitTimer != null) StopCoroutine(startEmitTimer);
        StartCoroutine(EmitCooldown());
        SoundManager.Instance.PlaySFX(emitEndSound);
        SoundManager.Instance.StopEmitBGM();
        EventManager.TriggerEvent("OnEmitFinished");
    }

    private void OnPlayerDieEmit()
    {
        isEmittable = false;
        emittedFrames = 0;
        foreach (IEmit obj in emitObjects)
        {
            obj.BeginEmit();
        }
        isEmitting = true;
        startEmitTimer = StartCoroutine(StartEmitTimer());
        SoundManager.Instance.PlaySFX(emitStartSound1);
        SoundManager.Instance.PlaySFX(emitStartSound2);
        StartCoroutine(emitClockSound());
        SoundManager.Instance.StartEmitBGM();
        EventManager.TriggerEvent("OnEmitStart");
    }

    //private void OnEmitEnd()
    //{
    //    foreach (IEmit obj in emitObjects)
    //    {
    //        obj.EndEmit();
    //    }
    //    emittedFrames = 0;
    //    isEmitting = false;
    //    TimeManager.Instance.InstantNormalTime();
    //    SoundManager.instance.StopEmitBGM();
    //    //StartCoroutine(emitCoolDownRoutine());
    //}

    public void EmitReset()
    {
        Debug.Log("Emit Reset");
        // 리셋
        if(startEmitTimer != null) StopCoroutine(startEmitTimer);

        foreach (IEmit obj in emitObjects)
        {
            obj.ResetFrame();
        }
        emitObjects.Clear();
        emittedFrames = 0;
        isEmitting = false;
        isEmittable = true;
        isEmitCanRecovery = true;
    }

    public void AddEmitTime(float time)
    {
        if (currentEmitableTime < emitDurationTime)
        {
            currentEmitableTime += time;
            if (currentEmitableTime >= emitDurationTime)
            {
                currentEmitableTime = emitDurationTime;
                if(isEmitCanRecovery) isEmittable = true;
            }
            updateEmitUI();
        }
    }

    private void updateEmitUI()
    {
        if(playerUIController != null)
            playerUIController.UpdateEmitBar(currentEmitableTime, emitDurationTime);
    }

    private IEnumerator StartEmitPrepareTimer()
    {
        yield return new WaitForSecondsRealtime(emitPreparationTime);
        StartEmit();
    }

    private IEnumerator StartEmitTimer()
    {
        while (currentEmitableTime >= 0)
        {
            currentEmitableTime -= Time.deltaTime;
            updateEmitUI();
            yield return null;
        }
        OnEmitFinished();
    }

    private IEnumerator EmitCooldown()
    {
        isEmitCanRecovery = false;
        float timer = 0f;
        while (timer < emitCoolTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        isEmitCanRecovery = true;
        if (currentEmitableTime == emitDurationTime)
        {
            isEmittable = true;
            updateEmitUI();
        }
    }

    private IEnumerator emitClockSound()
    {
        while (isEmitting)
        {
            SoundManager.Instance.PlaySFX(emitClockSound1);

            yield return new WaitForSeconds(1f);

            SoundManager.Instance.PlaySFX(emitClockSound2);

            yield return new WaitForSeconds(1f);
        }
    }
}
