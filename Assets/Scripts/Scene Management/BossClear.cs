using System.Collections;
using UnityEngine;

public class BossClear : MonoBehaviour
{
    [SerializeField] float slowTime = 1f;
    [SerializeField] float delayTime = 5f;

    private void OnEnable()
    {
        EventManager.StartListening("BossDead", startBossClear);
    }

    private void OnDisable()
    {
        EventManager.StopListening("BossDead", startBossClear);
    }


    private void startBossClear()
    {
        SoundManager.Instance.StopBGM();
        TimeManager.Instance.TimeStop();
        StartCoroutine(slowTimer());
        StartCoroutine(nextSceneDelayTimer());
    }
    private IEnumerator slowTimer()
    {
        yield return new WaitForSecondsRealtime(slowTime);
        TimeManager.Instance.TimeStart();
    }

    private IEnumerator nextSceneDelayTimer()
    {
        yield return new WaitForSecondsRealtime(delayTime);
        if(StageManager.Instance != null) StageManager.Instance.TryMoveToNextStage();
    }
}
