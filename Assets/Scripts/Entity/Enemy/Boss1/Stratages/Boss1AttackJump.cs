using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Strategy_JumpAttack", menuName = "Boss Strategy/Jump Attack")]
public class Boss1JumpAttack : Boss1AttackStrategy
{
    [Header("점프 공격 상세 설정")]
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float maxTrackingDistance = 8f;
    [SerializeField] private  AudioClip jumpReadySound;

    public override IEnumerator ExecuteAttackRoutine(EnemyBase enemy, System.Action<bool> setCancellable)
    {
        enemy.attackSound = attackSound;
        enemy.attackReadySound = attackReadySound;

        enemy.myAudioSource.PlayOneShot(attackReadySound);
        setCancellable(false);
        enemy.NotifyAttackWindupStarted();

        yield return new WaitForSeconds(startUpTime);
        enemy.Flip();

        Vector3 startPos = enemy.transform.position;
        Vector3 targetPos = startPos;

        if (enemy.target != null)
        {
            float targetX = enemy.target.position.x;
            float clampedX = Mathf.Clamp(targetX - startPos.x, -maxTrackingDistance, maxTrackingDistance);
            targetPos = new Vector3(startPos.x + clampedX, startPos.y, startPos.z);
        }

        float elapsedTime = 0f;
        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

            float heightOffset = 4f * jumpHeight * t * (1f - t);
            currentPos.y += heightOffset;

            enemy.transform.position = currentPos;
            yield return null;
        }

        enemy.transform.position = targetPos;

        enemy.NotifyAttackStrikeStarted();

        yield return new WaitForSeconds(recoveryTime);
    }
}