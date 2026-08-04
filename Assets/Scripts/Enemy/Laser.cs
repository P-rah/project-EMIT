using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : EnemyAttackHitbox
{
    private const float MaxAngleOffset = 5f;

    private BoxCollider2D _collider;
    private SpriteRenderer _sprender;
    private List<IDamageable> hitTargets; // 공격에 맞은 적들 리스트

    [SerializeField] private float loadTime = 1f; // 임시: 생성 전 대기시간
    [SerializeField] private float warnAreaTime = 2f; // 붉은색 경고 영역 시간
    [SerializeField] private bool isOnlyInstantDamage; // 즉시 데미지를 줄지, 영역에 지속될지
    [SerializeField] private float damageTime = 2f; // 실제로 데미지를 주는 시간
    [SerializeField] private float fadeOutTime = 1f; // 공격이 끝난 후 레이저가 사라지는 시간
    [SerializeField] private bool canDamage = false;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _sprender = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {   
        DisableDamage();
        // 처음 로드된 후에 일정 시간이 지나면 경고 표시를 하는 sprite renderer가 활성화된다
        StartCoroutine(PrepareLaser());
    }

    private IEnumerator PrepareLaser()
    {
        yield return new WaitForSeconds(loadTime);
        _sprender.enabled = true;
        SetLaserPath();
        yield return new WaitForSeconds(warnAreaTime);
        StartCoroutine(CastLaser());
    }

    private void SetLaserPath()
    {
        Transform player = Player.InstanceTransform;
        if (player == null)
        {
            Debug.LogError("Laser가 Player Transform을 찾지 못했습니다.", this);
            return;
        }

        Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = targetAngle + Random.Range(-MaxAngleOffset, MaxAngleOffset);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private IEnumerator CastLaser()
    {
        // 임시
        _sprender.color = Color.red;
        EnableDamage();
        yield return new WaitForSeconds(damageTime);
        DisableDamage();
        yield return new WaitForSeconds(fadeOutTime);
        Destroy(gameObject);
    }

    private void EnableDamage()
    {
        _sprender.enabled = true;
        _collider.enabled = true;
        canDamage = true;
    }

    private void DisableDamage()
    {
        _sprender.enabled = false;
        _collider.enabled = false;
        canDamage = false;
    }
}
