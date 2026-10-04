using UnityEngine;

/*
 * 개별 튜토리얼의 규칙 설정용 스크립터블 오브젝트
 */

public enum TutorialActionType
{
    Jump,
    Dash,
    Down,
    AttackSweep,
    AttackGrab,
    Emit,
    MoveLeft,
    MoveRight,
    Any
}

[CreateAssetMenu(fileName = "TutorialStep_", menuName = "Tutorial/Step Data")]
public class TutorialBase : ScriptableObject
{
    public string tutorialID;
    public int tutorialOrder;

    //[Header("다국어 키")]
    //public string messageKey;
    [TextArea] public string messageKey;

    public TutorialActionType requiredAction;
    public bool freezeTime = true;

    [Header("발동 설정")]
    [Tooltip("트리거 감지 후 튜토리얼 팝업이 뜨기까지의 대기 시간")]
    public float startDelay = 0f;
    public float minDisplayDuration = 0.5f;
}