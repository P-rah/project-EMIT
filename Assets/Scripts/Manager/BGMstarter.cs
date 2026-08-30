using UnityEngine;

public class BGMstarter : MonoBehaviour
{
    public AudioClip thisSceneBgm;

    private void Start()
    {
        // AudioManager.Instance.PlayBGM(thisSceneBgm);
        SoundManager.Instance.PlayBGM(thisSceneBgm);
    }
}
