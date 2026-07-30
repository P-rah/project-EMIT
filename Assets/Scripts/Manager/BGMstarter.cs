using UnityEngine;

public class BGMstarter : MonoBehaviour
{
    public AudioClip thisSceneBgm;

    private void Start()
    {
        SoundManager.instance.PlayBGM(thisSceneBgm);
    }
}
