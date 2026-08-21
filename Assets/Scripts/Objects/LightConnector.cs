using UnityEngine;
using UnityEngine.Rendering.Universal;

// 글로벌 라이트를 매니저에 연결하는 클래스
[RequireComponent(typeof(Light2D))]
public class LightConnector : MonoBehaviour
{
    private void Start()
    {
        if (LightManager.Instance != null)
        {
            LightManager.Instance.RegisterLight(GetComponent<Light2D>());
        }
    }
}