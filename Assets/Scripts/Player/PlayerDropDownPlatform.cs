using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDropDownPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler input;
    [SerializeField] private Collider2D playerCollider;

    [Header("Drop Through")]
    [SerializeField] private float ignoreDuration = 0.2f;
    [SerializeField] private float dropSpeed = 2f;

    [Header("Current Platform")]
    [SerializeField] private Collider2D _standingPlatform;
    private Rigidbody2D _rb;
    private bool _isDropping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
        }
    }
    
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsStandingOnPlatform(collision))
        {
            return;
        }

        _standingPlatform = collision.collider;
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!_isDropping && collision.collider == _standingPlatform)
        {
            _standingPlatform = null;
        }
    }

    private bool IsStandingOnPlatform(Collision2D collision)
    {
        // platform 타일맵만 usedByEffector를 true로 설정함
        return collision.collider.usedByEffector;
    }
    
    public void TryDropDown()
    {
        if (_isDropping || _standingPlatform == null)
        {
            return;
        }

        StartCoroutine(DropThroughRoutine(_standingPlatform));
    }

    private IEnumerator DropThroughRoutine(Collider2D platformCollider)
    {
        _isDropping = true;
        _standingPlatform = null;

        Physics2D.IgnoreCollision(
            playerCollider, platformCollider, true);

        // 실제로 아래쪽으로 빠져나가도록 최소 하강 속도 부여
        _rb.linearVelocity = new Vector2(
            _rb.linearVelocity.x,
            Mathf.Min(_rb.linearVelocity.y, -dropSpeed)
        );

        yield return new WaitForSeconds(ignoreDuration);

        if (platformCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                platformCollider,
                false
            );
        }

        _isDropping = false;
    }

    private static bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }
}