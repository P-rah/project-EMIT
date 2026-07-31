using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public TextMeshProUGUI enemyCount;
    public TextMeshProUGUI dialogueText;

    int previousCount;
    int count = 0;
    public int index = 0;

    string[] tutorialText = 
        {
        "이동: A,S \n점프: Space \n대쉬: Shift \n밀치기: 좌클릭 \n당기기: 우클릭",
        "밀치기/당기기 상태의 적은 \n반대 방향의 공격에 \n추가 피해를 입습니다",
        "E를 눌러 시간을 역행할 수 있습니다.\n역행 상태에서는 적의 이동 및 이전에 가한 넉백이\n거꾸로 적용됩니다.",
        "한번 역행을 한 후에는 적을 쓰러뜨리기 전까지\n다시 역행할 수 없습니다.",
        "역행 상태에서의 \n적의 넉백 반대 방향의 공격은 \n더욱 큰 피해를 입힙니다.",
        "역행 상태에서 적의 현재 넉백 방향과 \n같은 방향의 공격 후 반대 방향으로 공격하면 \n적은 즉사합니다"
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
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
        ShowCount();
        UpdateDialoque();
    }

    public void ShowCount()
    {
        if(SceneManager.GetActiveScene().buildIndex != 3)
        {
            count = StageManager.Instance.currentEnemyCount;
            enemyCount.text = $"남은 적 : {count}";
        }
        else enemyCount.text = "";
    }

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
