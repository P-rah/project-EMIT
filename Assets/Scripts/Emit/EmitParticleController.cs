using UnityEngine;

public class EmitParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem particle;

    void OnEnable()
    {
        EventManager.StartListening("OnEmitPressed", EnableParticles);
        EventManager.StartListening("StopEmit", DisableParticles);
        EventManager.StartListening("OnEmitFinished", DisableParticles);
    }

    void OnDisable()
    {
        EventManager.StopListening("OnEmitPressed", EnableParticles);
        EventManager.StopListening("StopEmit", DisableParticles);
        EventManager.StopListening("OnEmitFinished", DisableParticles);
    }

    void EnableParticles()
    {
        if (particle == null || EmitManager.Instance == null) return;

        if (EmitManager.Instance.isEmittable)
        {
            particle.Play();
        }
    }

    void DisableParticles()
    {
        if (particle == null) return;

        particle.Stop();
        particle.Clear();
    }
}
