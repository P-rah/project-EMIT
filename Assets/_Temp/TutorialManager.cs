using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    //public TextMeshProUGUI enemyCount;
    public TextMeshProUGUI dialogueText;

    int previousCount;
    int count = 0;
    public int index = 0;

    string[] tutorialText = 
        {
        "이동: A,S \n점프: Space \n대쉬: Shift \n밀치기: 좌클릭 \n당기기: 우클릭",
        "같은 방향으로 밀치기 공격이 적중하면 \n '몰아치기' 스택이 쌓입니다.",
        "현재 몰아치기의 반대 방향으로 밀치기 공격하면 스택을 소모해 더 큰 피해를 입힙니다.",
        "당기기 공격은 전방의 적을 당겨옵니다.\n 해당 공격은 몰아치기를 쌓지도, 소비하지도 않습니다.",
        "당기기 공격 직후의 밀치기 공격은 방향에 따라 적을 더욱 크게 밀칩니다. 또한 몰아치기의 방향이 밀치기 공격의 방향으로 변경됩니다.",
        "해당 공격으로 밀쳐지는 적에게 다른 적이 맞았다면, 해당 적이 가진 몰아치기 스택이 전이됩니다.",
        "E를 눌러 자신을 제외한 주변 물체의 시간을 역행할 수 있습니다. 역행되는 적은 체력을 제외한 모든 상태가 되감아집니다.",
        "역행 상태에서는 몰아치기의 방향이 반대가 됩니다. 또한 몰아치기가 소모되지 않습니다.",
        "역행은 "
        };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance.index = 0;
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Update()
    {
        //ShowCount();
        UpdateDialoque();
    }

    //public void ShowCount()
    //{
    //    if(SceneManager.GetActiveScene().buildIndex != 3)
    //    {
    //        count = StageManager.Instance.currentEnemyCount;
    //        enemyCount.text = $"남은 적 : {count}";
    //    }
    //    else enemyCount.text = "";
    //}

    private void ChangeDialogue(string message)
    {
        dialogueText.text = message;
    }

    private void UpdateDialoque()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (count != previousCount)
            {
                previousCount = count;
                if (index < tutorialText.Length) ChangeDialogue(tutorialText[index++]);
            }
        }
        else ChangeDialogue("");
    }
}
