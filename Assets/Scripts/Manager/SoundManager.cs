using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SoundManager>();

                if (instance == null)
                {
                    GameObject obj = new GameObject("SoundMananger");
                    instance = obj.AddComponent<SoundManager>();
                }
            }
            return instance;
        }
    }

    public AudioSource[] objectPoolSpeakers;

    public float SfxVolume => sfxVolume;
    public float BgmVolume => bgmVolume;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    private AudioSource bgmPlayer; // 배경음악 전용 (반복 재생)

    [Header("SFX")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    [Min(1)]
    [SerializeField] private int channels = 8;

    private AudioSource[] sfxPlayers;
    private int channelIndex;

    // 외부 3D 오디오 소스 구독을 위한 레지스트리
    private HashSet<AudioSource> registeredSfxSources = new HashSet<AudioSource>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        InitBgmPlayer();
        InitSfxPlayer();
    }

    // ==========================================
    // 외부 3D 오디오 소스 구독(Registry) 시스템
    // ==========================================
    public void RegisterSfxSource(AudioSource source)
    {
        if (source != null && !registeredSfxSources.Contains(source))
        {
            source.volume = sfxVolume; // 등록 직후 현재 SFX 볼륨 동기화
            registeredSfxSources.Add(source);
        }
    }

    public void UnregisterSfxSource(AudioSource source)
    {
        if (source != null)
        {
            registeredSfxSources.Remove(source);
        }
    }

    private void InitBgmPlayer()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform, false);

        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;
        bgmPlayer.outputAudioMixerGroup = bgmMixerGroup;
    }

    private void InitSfxPlayer()
    {
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.SetParent(transform, false);

        int channelCount = Mathf.Max(1, channels);
        sfxPlayers = new AudioSource[channelCount];

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;
            sfxPlayers[index].outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    public void PlayBGM(AudioClip clip, bool playReverse = false)
    {
        if (bgmPlayer == null || clip == null)
        {
            bgmPlayer.Stop();
            return;
        }

        if (bgmPlayer.clip == clip) return;

        if (playReverse) StartEmitBGM(); else StopEmitBGM();

        bgmPlayer.clip = clip;
        bgmPlayer.loop = true;
        bgmPlayer.Play();
    }

    public void StopBGM()
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.Stop();
        }
    }

    public void StopEmitBGM()
    {
        bgmPlayer.pitch = 1f;
    }

    public void StartEmitBGM()
    {
        bgmPlayer.pitch = -1f;
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        if (bgmPlayer == null) return;

        bgmPlayer.volume = bgmVolume;

        Debug.Log($"BGM Volume set to {bgmVolume}");
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxPlayers != null)
        {
            foreach (AudioSource player in sfxPlayers)
            {
                if (player != null) player.volume = sfxVolume;
            }
        }

        // 구독된 외부 3D 오디오 소스들의 볼륨도 함께 일괄 조절
        registeredSfxSources.RemoveWhere(source => source == null);
        foreach (AudioSource source in registeredSfxSources)
        {
            if (source != null)
            {
                source.volume = sfxVolume;
            }
        }

        Debug.Log($"SFX Volume set to {sfxVolume}");
    }

    public void PlaySFX(AudioClip clip, float pitchRandom = 0f)
    {
        if (clip == null || sfxPlayers == null || sfxPlayers.Length == 0)
        {
            return;
        }

        AudioSource player = GetAvailableSfxPlayer();

        //pitchRandom 값에 따라 랜덤하게 피치를 조절해줌
        player.pitch = 1.0f + Random.Range(-pitchRandom, pitchRandom);
        player.PlayOneShot(clip);
    }

    public void PauseAllSFX()
    {
        foreach (AudioSource sfx in sfxPlayers)
        {
            sfx.Pause();
        }
    }

    public void UnPauseAllSFX()
    {
        foreach (AudioSource sfx in sfxPlayers)
        {
            sfx.UnPause();
        }
    }

    private AudioSource GetAvailableSfxPlayer()
    {
        for (int offset = 0; offset < sfxPlayers.Length; offset++)
        {
            int index = (channelIndex + offset) % sfxPlayers.Length;

            if (sfxPlayers[index].isPlaying)
            {
                continue;
            }

            channelIndex = (index + 1) % sfxPlayers.Length;
            return sfxPlayers[index];
        }

        AudioSource fallbackPlayer = sfxPlayers[channelIndex];
        channelIndex = (channelIndex + 1) % sfxPlayers.Length;
        return fallbackPlayer;
    }
}