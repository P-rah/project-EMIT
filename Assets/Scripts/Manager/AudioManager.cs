using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;
    [Range(0f, 1f)] 
    [SerializeField] private float bgmVolume = 1f;
    
    private AudioSource bgmPlayer;

    [Header("SFX")]
    [Range(0f, 1f)] 
    [SerializeField] private float sfxVolume = 1f;
    [Min(1)] 
    [SerializeField] private int channels = 8;

    private AudioSource[] sfxPlayers;
    private int channelIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitBgmPlayer();
        InitSfxPlayer();
        PlayBgm(true);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
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

    public void PlayBgm(bool isPlay)
    {
        if (isPlay)
        {
            PlayBGM(bgmClip);
        }
        else
        {
            StopBGM();
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmPlayer == null || clip == null)
        {
            return;
        }

        bgmClip = clip;
        bgmPlayer.loop = true;

        if (bgmPlayer.clip != clip)
        {
            bgmPlayer.clip = clip;
        }

        if (!bgmPlayer.isPlaying)
        {
            bgmPlayer.Play();
        }
    }

    public void StopBGM()
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.Stop();
        }
    }

    public void StartEmitBGM()
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.pitch = -1f;
        }
    }

    public void StopEmitBGM()
    {
        if (bgmPlayer != null)
        {
            bgmPlayer.pitch = 1f;
        }
    }

    public void PlaySFX(AudioClip clip, float pitchRandom = 0f)
    {
        if (clip == null || sfxPlayers == null || sfxPlayers.Length == 0)
        {
            return;
        }

        AudioSource player = GetAvailableSfxPlayer();
        float pitchRange = Mathf.Abs(pitchRandom);

        player.pitch = 1f + Random.Range(-pitchRange, pitchRange);
        player.clip = clip;
        player.Play();
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
