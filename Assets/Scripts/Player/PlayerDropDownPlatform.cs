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
    private TempPlayerMovement _tempMovement;
    private PlayerInputHandler _input;
    private InputAction _downAction;
    private readonly HashSet<Collider2D> _ignoredPlatforms = new();
    private readonly List<Collider2D> _platformsToRestore = new();
    private readonly List<Collider2D> _overlappingPlatforms = new();
    private ContactFilter2D _platformFilter;
    private int _playerLayer;
    private int _platformLayer;
    private bool _layerOverrideActive;
    private bool _previousLayerIgnore;
    private bool _isDownHeld;
    public bool IsDropping;
    public bool HasStandingPlatform => _standingPlatform != null;
    public bool IsDropThroughHeld => _isDownHeld;
    public float DropSpeed => dropSpeed;
    private bool UsesTempMovement => _tempMovement != null && _tempMovement.isActiveAndEnabled;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _tempMovement = GetComponent<TempPlayerMovement>();
        _input = GetComponent<PlayerInputHandler>();

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

        _playerLayer = playerCollider != null ? playerCollider.gameObject.layer : gameObject.layer;
        _platformLayer = LayerMask.NameToLayer("Platform");
        _platformFilter = new ContactFilter2D { useTriggers = false };
        _platformFilter.SetLayerMask(_platformLayer >= 0 ? 1 << _platformLayer : 0);
    }

    private void FixedUpdate()
    {
        // Temp 이동에서는 이동 스크립트가 접지 판정 전에 직접 호출한다.
        if (!UsesTempMovement) TryDropDown();
    }

    private void OnTriggerStay2D (Collider2D other)
    {
        if (UsesTempMovement || !IsOneWayPlatform(other))
        {
            return;
        }

        _standingPlatform = other;
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (!UsesTempMovement && other == _standingPlatform)
        {
            _standingPlatform = null;
        }
    }

    public static bool IsOneWayPlatform(Collider2D other)
    {
        if (other == null || !other.usedByEffector) return false;
        PlatformEffector2D effector = other.GetComponent<PlatformEffector2D>();
        if (effector == null && other.attachedRigidbody != null)
            effector = other.attachedRigidbody.GetComponent<PlatformEffector2D>();
        return effector != null && effector.enabled && effector.useOneWay;
    }
    
    public void TryDropDown()
    {
        // 기존 PlayerMovement의 호출은 유지하되 Temp의 속도는 여기서 쓰지 않는다.
        if (!UsesTempMovement && PreparePhysicsStep(_standingPlatform))
            _rb.linearVelocityY = Mathf.Min(_rb.linearVelocityY, -dropSpeed);
    }

    private bool IsDownPressed()
    {
        return (_input == null || !_input.GameplayInputBlocked) &&
            _downAction != null && _downAction.IsPressed();
    }

    public bool PreparePhysicsStep(Collider2D support)
    {
        _standingPlatform = IsOneWayPlatform(support) ? support : null;
        _isDownHeld = IsDownPressed();
        bool startsDrop = _isDownHeld && _standingPlatform != null &&
            !_ignoredPlatforms.Contains(_standingPlatform);

        if (_isDownHeld)
        {
            if (!_layerOverrideActive && _platformLayer >= 0)
            {
                _previousLayerIgnore = Physics2D.GetIgnoreLayerCollision(_playerLayer, _platformLayer);
                Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, true);
                _layerOverrideActive = true;
            }

            if (_standingPlatform != null) IgnorePlatform(_standingPlatform);

            // 일반 지면에서 Down을 누른 채 진입하면 지지 플랫폼이 없으므로,
            // 새로 겹친 플랫폼도 키를 놓기 전에 개별 충돌 무시에 등록한다.
            IgnoreOverlappingPlatforms(playerCollider);
            IgnoreOverlappingPlatforms(feetCollider);
            _standingPlatform = null;
        }
        else
        {
            if (_layerOverrideActive)
            {
                // 전역 충돌을 복구하기 전에 겹친 발판만 개별 무시로 넘긴다.
                // 레이어 행렬과 무관한 명시적 필터로 현재 겹침을 조회한다.
                IgnoreOverlappingPlatforms(playerCollider);
                IgnoreOverlappingPlatforms(feetCollider);
                RestoreLayerCollision();
            }
            RestoreSeparatedPlatforms();
        }

        IsDropping = _ignoredPlatforms.Count > 0;
        return startsDrop;
    }

    private void IgnoreOverlappingPlatforms(Collider2D source)
    {
        if (source == null || !source.isActiveAndEnabled) return;
        source.Overlap(_platformFilter, _overlappingPlatforms);
        foreach (Collider2D platform in _overlappingPlatforms)
        {
            if (platform.attachedRigidbody != _rb) IgnorePlatform(platform);
        }
        _overlappingPlatforms.Clear();
    }

    private void RestoreLayerCollision()
    {
        if (!_layerOverrideActive) return;
        Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, _previousLayerIgnore);
        _layerOverrideActive = false;
    }

    private void IgnorePlatform(Collider2D platformCollider)
    {
        if (platformCollider == null ||
            !_ignoredPlatforms.Add(platformCollider))
        {
            return;
        }

        IsDropping = true;
        SetPlatformCollisionIgnored(platformCollider, true);
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
        return IsNearPlatform(playerCollider, platformCollider) ||
            IsNearPlatform(feetCollider, platformCollider);
    }

    private static bool IsNearPlatform(Collider2D source, Collider2D platform)
    {
        if (source == null || !source.isActiveAndEnabled || !platform.isActiveAndEnabled) return false;
        ColliderDistance2D distance = source.Distance(platform);
        return distance.isValid && distance.distance <= Physics2D.defaultContactOffset * 2f;
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
        _standingPlatform = null;
        RestoreLayerCollision();
        RestoreIgnoredPlatforms();
    }
}
