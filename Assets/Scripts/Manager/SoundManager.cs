using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource[] objectPoolSpeakers;

    [Header("스피커 (Audio Source)")]
    public AudioSource bgmPlayer; // 배경음악 전용 (반복 재생)
    public AudioSource sfxPlayer; // 플레이어 효과음 전용 (반복 재생 없음)


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmClip == null) return;
        if (bgmPlayer.clip == bgmClip) return;

        bgmPlayer.clip = bgmClip;
        bgmPlayer.loop = true;
        bgmPlayer.Play();
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public void StopEmitBGM()
    {
        bgmPlayer.pitch = 1f;
    }

    public void StartEmitBGM()
    {
        bgmPlayer.pitch = -1f;
    }

    public void PlaySFX(AudioClip clip, float pitchRandom = 0f)
    {
        //pitchRandom 값에 따라 랜덤하게 피치를 조절해줌
        sfxPlayer.pitch = 1.0f + Random.Range(-pitchRandom, pitchRandom);
        sfxPlayer.PlayOneShot(clip);
        sfxPlayer.pitch = 1.0f;
    }
}
