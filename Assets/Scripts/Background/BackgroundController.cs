using Unity.VisualScripting;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float _initialPosX;
    private float _initialPosY;
    private float _backgroundWidth;

    [SerializeField] private Vector2 parallaxEffect = new Vector2(1f, 0f);

    [SerializeField] public GameObject targetCamera;

    void Start()
    {
        _initialPosX = transform.position.x;
        _initialPosY = transform.position.y;
        _backgroundWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        float distanceHorizontal = targetCamera.transform.position.x * parallaxEffect.x;
        float distanceVertical = targetCamera.transform.position.y * parallaxEffect.y;
        float horizontalMovement = targetCamera.transform.position.x * (1 - parallaxEffect.x);

        transform.position = new Vector3(_initialPosX + distanceHorizontal, _initialPosY + distanceVertical, transform.position.z);

        ResetInitialPos(horizontalMovement);
    }

    private void ResetInitialPos(float horizontalMovement)
    {
        if (horizontalMovement > _initialPosX + _backgroundWidth)
        {
            _initialPosX += _backgroundWidth;
        }
        else if (horizontalMovement < _initialPosX - _backgroundWidth)
        {
            _initialPosX -= _backgroundWidth;
        }
    }
}
/*

    이후에 DefaultExecutionOrder 사용하지 않을 시, 

    using Cinemachine;
    using UnityEngine;

    public class BackgroundController : MonoBehaviour
    {
        private float initialPosX;

        public GameObject cam;
        public float parallaxEffect;

        private void Awake()
        {
            initialPosX = transform.position.x;
        }

        private void OnEnable()
        {
            CinemachineCore.CameraUpdatedEvent.AddListener(UpdateParallax);
        }

        private void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdateParallax);
        }

        private void UpdateParallax(CinemachineBrain brain)
        {
            if (brain.OutputCamera == null ||
                brain.OutputCamera.gameObject != cam)
                return;

            float distance = cam.transform.position.x * parallaxEffect;

            transform.position = new Vector2(
                initialPosX + distance,
                transform.position.y
            );
        }
    }
*/