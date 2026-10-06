using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    // [SerializeField] private TextMeshProUGUI _loadingText;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 1f;

    private void Awake()
    {   
        canvasGroup = GetComponent<CanvasGroup>();
        // _loadingText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public async UniTask Show()
    {
        if (canvasGroup == null)
        {
            Awake();
        }

        gameObject.SetActive(true);
        Debug.Log("로딩화면 뜸");
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        // _loadingText.alpha = 1f;

        await canvasGroup.DOFade(1f, fadeInTime)
        .SetUpdate(true)
        .SetEase(Ease.OutQuad)
        .AsyncWaitForCompletion();
    }

    public async UniTask Hide()
    {
        await canvasGroup.DOFade(0f, fadeOutTime)
        .SetUpdate(true)
        .SetEase(Ease.OutQuad)
        .AsyncWaitForCompletion();
        
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        
        Debug.Log("로딩화면 꺼짐");
    }
}
