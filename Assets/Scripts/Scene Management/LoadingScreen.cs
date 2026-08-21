using UnityEngine;
using DG.Tweening;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 1f;

    private void Awake()
    {   
        canvasGroup = GetComponent<CanvasGroup>();
        _loadingText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Debug.Log("로딩화면 뜸");
        
        _loadingText.alpha = 1f;
        canvasGroup.DOFade(1f, fadeInTime)
        .SetUpdate(true)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = true;
        }
        );
        
    }

    public void Hide()
    {
        _loadingText.alpha = 0f;
        canvasGroup.DOFade(0f, fadeOutTime)
        .SetUpdate(true)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            Debug.Log("로딩화면 꺼짐");
        }
        );
    }
}
