using System.Collections;
using UnityEngine;

public abstract class Boss1AttackStrategy : ScriptableObject
{
    [Header("공통 공격 설정")]
    public float startUpTime = 0.5f;   // 선딜레이
    public float recoveryTime = 0.5f;  // 후딜레이
    public AudioClip attackReadySound;      // 공격 준비 사운드
    public AudioClip attackSound;      // 공격 사운드
    public float cameraShakeIntensity = 10f;
    public float cameraShakeDuration = 0.2f;

    [Header("애니메이션 설정 (Animator State 이름)")]
    public string windupAnimName = "TempName";
    public string strikeAnimName = "TempName";

    [Header("자식 이펙트 설정")]
    [Tooltip("보스 하위 자식 오브젝트 중 활성화할 이펙트의 GameObject 이름")]
    public string childEffectName = "TempName";

    public virtual bool CanExecute(EnemyBase enemy) => true;

    /// <summary>
    /// 실제 공격 코루틴 실행
    /// </summary>
    /// <param name="enemy">공격하는 보스 참조</param>
    /// <param name="setCancellable">State의 isCancellable을 변경할 액션 콜백</param>
    public abstract IEnumerator ExecuteAttackRoutine(EnemyBase enemy, System.Action<bool> setCancellable);
}