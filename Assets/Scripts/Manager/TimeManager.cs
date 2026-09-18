using UnityEngine;
using System.Collections;

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

    [Header("시간 설정")]
    [SerializeField] private float normalTimeScale = 1f;
    [SerializeField] private float slowTimeScale = 0f;

    [Header("전환 시간")]
    [SerializeField] private float timeToSlowDown = 1f;
    [SerializeField] private float timeToSpeedUp = 1f;

    private float targetTimeScale = 1f;
    private float transitionSpeed;

    private void Start()
    {
        targetTimeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }

    private void Update()
    {
        if (!timeChangeable) return;
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

    public void InstantNormalTime()
    {
        if (!timeChangeable) return;
        targetTimeScale = normalTimeScale;
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }

    public void StopTimeFor(float durationTime)
    {
        StartCoroutine(StopTimeRoutine(durationTime));
    }

    private IEnumerator StopTimeRoutine(float duration)
    {
        timeChangeable = false;
        Time.timeScale = 0.1f;

        yield return new WaitForSecondsRealtime(duration);

        timeChangeable = true;
        InstantNormalTime();
    }
}
