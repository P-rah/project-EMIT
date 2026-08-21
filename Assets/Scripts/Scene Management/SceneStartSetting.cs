using UnityEngine;

public class SceneStartSetting : MonoBehaviour
{
    [SerializeField] private GameManager.GameState gameState;
    [SerializeField] private AudioClip thisSceneBgm;
    [SerializeField] private bool noBGM = false;
    [SerializeField] private bool revserse = false;

    void Start()
    {
        GameManager.Instance.ChangeState(gameState);
        if (noBGM) SoundManager.instance.StopBGM();
        else if(thisSceneBgm != null) SoundManager.instance.PlayBGM(thisSceneBgm, revserse);
    }
}
