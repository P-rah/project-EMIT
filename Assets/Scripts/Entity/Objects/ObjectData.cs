using UnityEngine;

[CreateAssetMenu(fileName = "Object Data", menuName = "Scriptable Object/ObjectData")]
public class ObjectData : ScriptableObject
{
    [Header("설명")]
    public string enemyName;
    public string enemyInfo;

    [Header("넉백 관련")]
    public float knockbackRate;
    public float strongKnockbackRate;
    public float knockbackTime;
    public float HitRecoveryTime;

    [Header("파워 관련")]
    public float powerResetTime;
    public int powerStackWeight;

    [Header("사운드 관련")]
    public AudioClip sweepHitSound;
    public AudioClip grabHitSound;
}
