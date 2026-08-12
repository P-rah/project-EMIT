using UnityEngine;

public class Laser : EnemyAttackHitbox
{
    [Header("Settings")]
    public string poolTag = "BossLaser";
    public float lifeTime = 0.2f;
    public AudioSource myAudioSource;
    public AudioClip laserSound;

    private void Awake()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Invoke("Deactivate", lifeTime);
        myAudioSource.PlayOneShot(laserSound);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void Deactivate()
    {
        ObjectPoolManager.Instance.ReturnToPool(poolTag, gameObject);
    }
}
