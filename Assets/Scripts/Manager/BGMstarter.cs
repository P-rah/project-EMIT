using UnityEngine;

public class BGMstarter : MonoBehaviour
{
    public AudioClip thisSceneBgm;

    private void Start()
    {
        // AudioManager.Instance.PlayBGM(thisSceneBgm);
        SoundManager.instance.PlayBGM(thisSceneBgm);
    }
}
