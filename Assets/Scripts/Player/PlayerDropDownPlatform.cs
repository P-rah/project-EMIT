using System.Collections;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDropDownPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private CircleCollider2D feetCollider;

    [Header("Drop Through")]
    [SerializeField] private float ignoreDuration = 0.2f;
    [SerializeField] private float dropSpeed = 2f;

    [Header("Current Platform")]
    [SerializeField] private Collider2D _standingPlatform;
    private Rigidbody2D _rb;
    public bool IsDropping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider2D>();
        }
        if (feetCollider == null)
        {
            feetCollider = GetComponentInChildren<CircleCollider2D>();
        }
    }

    

    private void OnTriggerStay2D (Collider2D other)
    {
        if (!IsStandingOnPlatform(other))
        {
            return;
        }

        _standingPlatform = other;
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsDropping && other == _standingPlatform)
        {
            _standingPlatform = null;
        }
    }

    private bool IsStandingOnPlatform(Collider2D other)
    {
        // platform 타일맵만 usedByEffector를 true로 설정함
        return other.usedByEffector;
    }
    
    public void TryDropDown()
    {
        if (IsDropping || _standingPlatform == null)
        {
            return;
        }

        StartCoroutine(DropThroughRoutine(_standingPlatform));
    }

    private IEnumerator DropThroughRoutine(Collider2D platformCollider)
    {
        IsDropping = true;
        _standingPlatform = null;

        Physics2D.IgnoreCollision(
            playerCollider, platformCollider, true);

        Physics2D.IgnoreCollision(
                feetCollider, platformCollider, true);

        // 실제로 아래쪽으로 빠져나가도록 최소 하강 속도 부여
        _rb.linearVelocity = new Vector2(
            _rb.linearVelocity.x,
            Mathf.Min(_rb.linearVelocity.y, -dropSpeed)
        );

        yield return new WaitForSeconds(ignoreDuration);

        if (platformCollider != null && feetCollider != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                platformCollider,
                false
            );
            Physics2D.IgnoreCollision(
                feetCollider,
                platformCollider,
                false
            );
        }

        IsDropping = false;
    }
}