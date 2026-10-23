using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Components;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("참조 연결")]
    [SerializeField] private PlayerInputHandler playerInput;
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("Localization 설정")]
    [SerializeField] private string tableName = "Tutorial Text Table";

    private TimeManager timeManager;
    private LightManager lightManager;
    private TutorialBase currentStep;

    private LocalizeStringEvent localizeStringEvent;

    private bool isTutorialActive = false;
    private bool isPendingDelay = false;
    private bool canComplete = false;
    private int currentTutorialOrder = 1;

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

        if (guideText != null)
        {
            if (!guideText.TryGetComponent(out localizeStringEvent))
            {
                localizeStringEvent = guideText.gameObject.AddComponent<LocalizeStringEvent>();
            }

            localizeStringEvent.OnUpdateString.AddListener((translatedText) =>
            {
                guideText.text = translatedText;
            });
        }
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

    public bool CanStartTutorial(TutorialBase step)
    {
        if (step == null || isTutorialActive || isPendingDelay) return false;
        return currentTutorialOrder == step.tutorialOrder;
    }

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
        canComplete = false;

        if (playerInput != null && currentStep.requiredAction != TutorialActionType.Any)
        {
            playerInput.SetAllowedAction(currentStep.requiredAction);
        }

        if (localizeStringEvent != null && !string.IsNullOrEmpty(currentStep.messageKey))
        {
            localizeStringEvent.StringReference.SetReference(tableName, currentStep.messageKey);
            localizeStringEvent.RefreshString();
        }

        if (currentStep.freezeTime)
        {
            if (timeManager != null) timeManager.Pause();
            if (lightManager != null) lightManager.SetLightInstantly(0.1f);
        }

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
            canComplete = true;
        }
    }

    private IEnumerator UnlockInputRoutine(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        canComplete = true;
        unlockInputCoroutine = null;
    }

    // ========================================================================
    // 3. 입력 수신 및 완료 처리
    // ========================================================================

    private void HandlePlayerAction(TutorialActionType action)
    {
        if (!isTutorialActive || currentStep == null || !canComplete) return;

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
            guideText.text = string.Empty;
        }

        if (localizeStringEvent != null)
        {
            localizeStringEvent.StringReference.Clear();
        }

        if (currentStep != null && currentStep.freezeTime)
        {
            if (timeManager != null) timeManager.Resume();
            if (lightManager != null) lightManager.SetLightInstantly(1f);
        }

        isTutorialActive = false;
        isPendingDelay = false;
        canComplete = false;
        currentStep = null;
        currentTutorialOrder++;

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