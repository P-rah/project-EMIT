using Cinemachine;
using UnityEngine;

/*
 * 게임 플레이 가상 카메라를 흔드는 싱글톤 클래스
 * CameraShake.Instance.ShakeCamera(강도f, 시간f);
 * 으로 호출
 */
[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        virtualCameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        if (virtualCameraNoise != null)
        {
            virtualCameraNoise.m_AmplitudeGain = 0f;
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        if (virtualCameraNoise == null) return;

        virtualCameraNoise.m_AmplitudeGain = intensity;

        startingIntensity = intensity;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            if (virtualCameraNoise != null)
            {
                virtualCameraNoise.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1f - (shakeTimer / shakeTimerTotal));
            }
        }
    }
}
