using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDropDownPlatform : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private CircleCollider2D feetCollider;

    [Header("Drop Through")]
    [SerializeField] private float dropSpeed = 2f;

    [Header("Current Platform")]
    [SerializeField] private Collider2D _standingPlatform;
    private Rigidbody2D _rb;
    private InputAction _downAction;
    private readonly HashSet<Collider2D> _ignoredPlatforms = new();
    private readonly List<Collider2D> _platformsToRestore = new();
    private bool _isDownHeld;
    public bool IsDropping;
    public bool HasStandingPlatform => _standingPlatform != null;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        PlayerInput playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            _downAction = playerInput.actions.FindAction("Down");
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider2D>();
        }
        if (feetCollider == null)
        {
            feetCollider = GetComponentInChildren<CircleCollider2D>();
        }
    }

    private void Update()
    {
        SetDropThrough(IsDownPressed());
    }

    private void FixedUpdate()
    {
        if (!_isDownHeld)
        {
            RestoreSeparatedPlatforms();
        }
    }

    private void OnTriggerStay2D (Collider2D other)
    {
        if (!IsStandingOnPlatform(other))
        {
            return;
        }

        _standingPlatform = other;

        if (_isDownHeld)
        {
            IgnorePlatform(other);
        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _standingPlatform)
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
        SetDropThrough(IsDownPressed());
    }

    private bool IsDownPressed()
    {
        return _downAction != null && _downAction.IsPressed();
    }

    private void SetDropThrough(bool isDownHeld)
    {
        _isDownHeld = isDownHeld;

        if (!_isDownHeld)
        {
            return;
        }

        if (_standingPlatform != null)
        {
            IgnorePlatform(_standingPlatform);
        }
    }

    private void IgnorePlatform(Collider2D platformCollider)
    {
        if (platformCollider == null ||
            !_ignoredPlatforms.Add(platformCollider))
        {
            return;
        }

        IsDropping = true;
        _standingPlatform = null;

        SetPlatformCollisionIgnored(platformCollider, true);

        // 실제로 아래쪽으로 빠져나가도록 최소 하강 속도 부여
        _rb.linearVelocity = new Vector2(
            _rb.linearVelocity.x,
            Mathf.Min(_rb.linearVelocity.y, -dropSpeed)
        );
    }

    private void RestoreSeparatedPlatforms()
    {
        if (_ignoredPlatforms.Count == 0)
        {
            IsDropping = false;
            return;
        }

        _platformsToRestore.Clear();

        foreach (Collider2D platformCollider in _ignoredPlatforms)
        {
            if (platformCollider == null || !IsOverlappingPlatform(platformCollider))
            {
                _platformsToRestore.Add(platformCollider);
            }
        }

        foreach (Collider2D platformCollider in _platformsToRestore)
        {
            if (platformCollider != null)
            {
                SetPlatformCollisionIgnored(platformCollider, false);
            }

            _ignoredPlatforms.Remove(platformCollider);
        }

        _platformsToRestore.Clear();
        IsDropping = _ignoredPlatforms.Count > 0;
    }

    private bool IsOverlappingPlatform(Collider2D platformCollider)
    {
        return playerCollider != null &&
            playerCollider.Distance(platformCollider).isOverlapped;
    }

    private void RestoreIgnoredPlatforms()
    {
        if (_ignoredPlatforms.Count == 0)
        {
            IsDropping = false;
            return;
        }

        foreach (Collider2D platformCollider in _ignoredPlatforms)
        {
            SetPlatformCollisionIgnored(platformCollider, false);
        }

        _ignoredPlatforms.Clear();
        IsDropping = false;
    }

    private void SetPlatformCollisionIgnored(
        Collider2D platformCollider,
        bool ignore)
    {
        if (platformCollider == null)
        {
            return;
        }

        if (playerCollider != null)
        {
            Physics2D.IgnoreCollision(
                playerCollider,
                platformCollider,
                ignore
            );
        }

        if (feetCollider != null)
        {
            Physics2D.IgnoreCollision(
                feetCollider,
                platformCollider,
                ignore
            );
        }
    }

    private void OnDisable()
    {
        _isDownHeld = false;
        RestoreIgnoredPlatforms();
    }
}
