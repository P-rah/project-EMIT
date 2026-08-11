using UnityEngine;
using DG.Tweening;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [SerializeField] private TextMeshProUGUI _loadingText;

    private void Awake()
    {   
        canvasGroup = GetComponent<CanvasGroup>();
        _loadingText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _loadingText.alpha = 0f;
        canvasGroup.DOFade(0f, 1f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        );
    }
}
