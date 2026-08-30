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
        if (noBGM) SoundManager.Instance.StopBGM();
        else if(thisSceneBgm != null) SoundManager.Instance.PlayBGM(thisSceneBgm, revserse);
    }
}
