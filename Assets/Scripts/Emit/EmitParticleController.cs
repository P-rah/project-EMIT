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

    void EnableParticles()
    {
        if (EmitManager.Instance.isEmittable)
        {
            particle.Play();
        }
    }

    void DisableParticles()
    {
        particle.Stop();
        particle.Clear();
    }
}
