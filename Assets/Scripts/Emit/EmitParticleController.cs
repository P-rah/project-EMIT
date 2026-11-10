using UnityEngine;

public class EmitParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;

    void OnEnable()
    {
        EventManager.StartListening("OnEmitStart", EnableParticles);
        EventManager.StartListening("StopEmit", DisableParticles);
        EventManager.StartListening("OnEmitFinished", DisableParticles);
    }

    void OnDisable()
    {
        EventManager.StopListening("OnEmitStart", EnableParticles);
        EventManager.StopListening("StopEmit", DisableParticles);
        EventManager.StopListening("OnEmitFinished", DisableParticles);
    }

    void EnableParticles()
    {
        if (particle == null || EmitManager.Instance == null) return;

        if(EmitManager.Instance.isEmitting)
        {
            particle.Play();
        }
    }

    void DisableParticles()
    {
        if (particle == null) return;

        particle.Stop();
        particle.Clear();
        Debug.Log("파티클 소멸");
    }
}