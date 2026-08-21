using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("튜토리얼 세팅")]
    public TextMeshProUGUI tutorialText;
    public bool pauseGame = false;
    public bool triggerOnce = true;
    public string message;

    private bool hasTriggered = false;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerOnce && hasTriggered) return;

        if (collision.CompareTag("Player"))
        {
            hasTriggered = true;

            if (tutorialText != null) tutorialText.text = message;

            if (pauseGame) Time.timeScale = 0.1f;
        }
    }

    public void CompleteTutorial()
    {
        if (tutorialText != null) tutorialText.text = "";
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}
