using UnityEngine;

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

    [Header("시간 설정")]
    public float normalTimeScale = 1f;
    public float slowTimeScale = 0.3f;

    [Header("전환 시간")]
    public float timeToSlowDown = 1f;
    public float timeToSpeedUp = 1f;

    private float targetTimeScale = 1f;
    private float transitionSpeed;

    private void Start()
    {
        targetTimeScale = normalTimeScale;
    }

    private void Update()
    {
        if (Mathf.Abs(Time.timeScale - targetTimeScale) > 0.001f)
        {
            Time.timeScale = Mathf.MoveTowards(
                Time.timeScale,
                targetTimeScale,
                transitionSpeed * Time.unscaledDeltaTime
            );
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = targetTimeScale;
        }
    }

    public void StartSlowMotion()
    {
        targetTimeScale = slowTimeScale;
        transitionSpeed = Mathf.Abs(normalTimeScale - slowTimeScale) / timeToSlowDown;
    }

    public void StopSlowMotion()
    {
        targetTimeScale = normalTimeScale;
        transitionSpeed = Mathf.Abs(normalTimeScale - slowTimeScale) / timeToSpeedUp;
    }

    public void InstantNormalTime()
    {
        targetTimeScale = normalTimeScale;
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;
    }
}
