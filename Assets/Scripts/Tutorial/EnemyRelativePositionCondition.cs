using UnityEngine;

public enum PositionCheckType
{
    DistanceOnly,   // 단순 거리만 검사
    ToTheLeft,      // 플레이어가 적의 왼쪽에 위치 (월드 X축 -)
    ToTheRight      // 플레이어가 적의 오른쪽에 위치 (월드 X축 +)
}

[System.Serializable]
[CreateAssetMenu(fileName = "Cond_RelativePos_", menuName = "Tutorial/Condition/Relative Position")]
public class EnemyRelativePositionCondition : TutorialCondition
{
    [Header("검사 유형")]
    [SerializeField] private PositionCheckType checkType = PositionCheckType.DistanceOnly;

    [Header("거리 조건")]
    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float maxDistance = 5f;

    public override bool IsMet(EnemyBase enemy)
    {
        if (enemy == null) return false;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return false;

        Transform playerTrans = playerObj.transform;
        Transform enemyTrans = enemy.transform;

        float currentDistance = Vector3.Distance(enemyTrans.position, playerTrans.position);
        if (currentDistance < minDistance || currentDistance > maxDistance)
        {
            return false;
        }

        float xDifference = playerTrans.position.x - enemyTrans.position.x;

        switch (checkType)
        {
            case PositionCheckType.DistanceOnly:
                return true;

            case PositionCheckType.ToTheLeft:
                return xDifference < 0f;

            case PositionCheckType.ToTheRight:
                return xDifference > 0f;

            default:
                return false;
        }
    }
}