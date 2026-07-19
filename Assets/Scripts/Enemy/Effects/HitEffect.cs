using UnityEngine;

public class HitEffect : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnParticleSystemStopped()
    {
        EffectManager.Instance.ReturnEffect(gameObject);
    }
}
