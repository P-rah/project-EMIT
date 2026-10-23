using UnityEngine;
using System.Collections;
using Unity.AppUI.Redux.DevTools;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private bool timeChangeable = true;
    private Coroutine stopTimeRoutine;
    private float stopTimeRemaining;

    [Header("시간 설정")]
    [SerializeField] private float normalTimeScale = 1f;
    [SerializeField] private float slowTimeScale = 0.1f;

    [Header("전환 시간")]
    [SerializeField] private float timeToSlowDown = 0f;
    [SerializeField] private float timeToSpeedUp = 1f;

    public bool IsPaused { get; private set;}
    public event System.Action Paused;
    private float storedTimeScale = 0f; // 일시정지 시 저장해두는 직전 프레임의 TimeScale

    private float targetTimeScale = 1f;
    private float transitionSpeed;

    private void Start()
    {
        targetTimeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }

    private void Update()
    {
        if (IsPaused || !timeChangeable) return;
        if (Mathf.Abs(Time.timeScale - targetTimeScale) > 0.001f)
        {
            Time.timeScale = Mathf.MoveTowards(
                Time.timeScale,
                targetTimeScale,
                transitionSpeed * Time.unscaledDeltaTime
            );
        }
        else
        {
            Time.timeScale = targetTimeScale;
        }
    }

    public void TimeStop()
    {
        targetTimeScale = slowTimeScale;
        transitionSpeed = Mathf.Abs(normalTimeScale - slowTimeScale) / timeToSlowDown;
    }

    public void TimeStart()
    {
        targetTimeScale = normalTimeScale;
        transitionSpeed = Mathf.Abs(normalTimeScale - slowTimeScale) / timeToSpeedUp;
    }

    public void Pause()
    {
        if (IsPaused) return;
        storedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        IsPaused = true;
        Paused?.Invoke();
    }

    public void Resume()
    {
        if (!IsPaused) return;
        Time.timeScale = storedTimeScale;
        IsPaused = false;
    }

    public void InstantNormalTime()
    {
        if (!timeChangeable) return;
        targetTimeScale = normalTimeScale;
        if (IsPaused) storedTimeScale = normalTimeScale;
        else Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }

    // 슬로모션과 무관하게 진행하되, Pause 동안에는 남은 시간을 보존한다.
    public IEnumerator WaitForGameplaySeconds(float duration)
    {
        float remaining = duration;
        while (remaining > 0f || IsPaused)
        {
            yield return null;
            if (!IsPaused) remaining -= Time.unscaledDeltaTime;
        }
    }

    public void StopTimeFor(float durationTime)
    {
        stopTimeRemaining = Mathf.Max(stopTimeRemaining, durationTime);
        if (stopTimeRoutine == null)
            stopTimeRoutine = StartCoroutine(StopTimeRoutine());
    }

    private IEnumerator StopTimeRoutine()
    {
        timeChangeable = false;
        if (IsPaused) storedTimeScale = 0.1f;
        else Time.timeScale = 0.1f;

        do
        {
            yield return null;
            if (!IsPaused) stopTimeRemaining -= Time.unscaledDeltaTime;
        }
        while (stopTimeRemaining > 0f || IsPaused);

        stopTimeRemaining = 0f;
        stopTimeRoutine = null;
        timeChangeable = true;
        InstantNormalTime();
    }
}
