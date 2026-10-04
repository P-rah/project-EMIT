using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("참조 연결")]
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private TextMeshProUGUI guideText;

    // 외부 매니저 및 상태 변수
    private TimeManager timeManager;
    private LightManager lightManager;
    private TutorialBase currentStep;

    private bool isTutorialActive = false;
    private bool isPendingDelay = false;
    private bool canComplete = false; // 최소 대기 시간 충족 여부
    private int currentTutorialOrder = 1;

    // 코루틴 핸들
    private Coroutine startDelayCoroutine;
    private Coroutine unlockInputCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentTutorialOrder = 1;
    }

    private void Start()
    {
        timeManager = TimeManager.Instance;
        lightManager = LightManager.Instance;
    }

    private void OnEnable()
    {
        if (playerInput != null)
        {
            playerInput.OnActionPerformed += HandlePlayerAction;
        }
        EventManager.StartListening("OnEmitPressed", OnEmitPressedInTutorial);
    }

    private void OnDisable()
    {
        if (playerInput != null)
        {
            playerInput.OnActionPerformed -= HandlePlayerAction;
        }
        EventManager.StopListening("OnEmitPressed", OnEmitPressedInTutorial);
    }

    // ========================================================================
    // 1. 순서 검사 및 요청 처리
    // ========================================================================

    /// <summary>
    /// 지정한 튜토리얼이 지금 실행될 차례인지 검사
    /// </summary>
    public bool CanStartTutorial(TutorialBase step)
    {
        if (step == null || isTutorialActive || isPendingDelay) return false;
        return currentTutorialOrder == step.tutorialOrder;
    }

    /// <summary>
    /// 외부 트리거에서 튜토리얼 실행 요청 시 호출
    /// </summary>
    public bool TryStartTutorial(TutorialBase step)
    {
        if (!CanStartTutorial(step)) return false;

        if (step.startDelay > 0f)
        {
            startDelayCoroutine = StartCoroutine(StartTutorialWithDelayRoutine(step));
        }
        else
        {
            StartTutorial(step);
        }

        return true;
    }

    private IEnumerator StartTutorialWithDelayRoutine(TutorialBase step)
    {
        isPendingDelay = true;

        yield return new WaitForSecondsRealtime(step.startDelay);

        isPendingDelay = false;
        startDelayCoroutine = null;

        StartTutorial(step);
    }

    // ========================================================================
    // 2. 튜토리얼 실행 및 연동 매니저 제어
    // ========================================================================

    private void StartTutorial(TutorialBase step)
    {
        currentStep = step;
        isTutorialActive = true;
        canComplete = false; // 튜토리얼 시작 시 입력 완료 차단

        // 입력 제한 설정
        if (playerInput != null && currentStep.requiredAction != TutorialActionType.Any)
        {
            playerInput.SetAllowedAction(currentStep.requiredAction);
        }

        // 가이드 텍스트 출력
        if (guideText != null)
        {
            guideText.text = currentStep.messageKey;
        }

        // Time / Light 매니저 연동
        if (currentStep.freezeTime)
        {
            if (timeManager != null) timeManager.Pause();
            if (lightManager != null) lightManager.SetLightInstantly(0.1f);
        }

        // minDisplayDuration 대기 코루틴 가동
        if (unlockInputCoroutine != null)
        {
            StopCoroutine(unlockInputCoroutine);
        }

        if (currentStep.minDisplayDuration > 0f)
        {
            unlockInputCoroutine = StartCoroutine(UnlockInputRoutine(currentStep.minDisplayDuration));
        }
        else
        {
            canComplete = true; // 대기 시간이 0 이하라면 즉시 완료 가능
        }
    }

    private IEnumerator UnlockInputRoutine(float duration)
    {
        // 시간을 멈추는 freezeTime 옵션이 있을 수 있으므로 Realtime 사용
        yield return new WaitForSecondsRealtime(duration);
        canComplete = true;
        unlockInputCoroutine = null;
    }

    // ========================================================================
    // 3. 입력 수신 및 완료 처리
    // ========================================================================

    private void HandlePlayerAction(TutorialActionType action)
    {
        // 아직 대기 시간(minDisplayDuration)이 안 지났으면 완료 불가
        if (!isTutorialActive || currentStep == null || !canComplete) return;

        Debug.Log("이거 호출됨");

        if (currentStep.requiredAction == action || currentStep.requiredAction == TutorialActionType.Any)
        {
            CompleteTutorial();
        }
    }

    private void OnEmitPressedInTutorial()
    {
        HandlePlayerAction(TutorialActionType.Emit);
    }

    private void CompleteTutorial()
    {
        if (playerInput != null)
        {
            playerInput.ClearAllowedAction();
        }

        if (guideText != null)
        {
            guideText.text = null;
        }

        // Time / Light 매니저 복원
        if (currentStep != null && currentStep.freezeTime)
        {
            if (timeManager != null) timeManager.Resume();
            if (lightManager != null) lightManager.SetLightInstantly(1f);
        }

        // 진행 상태 갱신
        isTutorialActive = false;
        isPendingDelay = false;
        canComplete = false;
        currentStep = null;
        currentTutorialOrder++;

        // 실행 중인 코루틴 정리
        if (startDelayCoroutine != null)
        {
            StopCoroutine(startDelayCoroutine);
            startDelayCoroutine = null;
        }

        if (unlockInputCoroutine != null)
        {
            StopCoroutine(unlockInputCoroutine);
            unlockInputCoroutine = null;
        }
    }
}