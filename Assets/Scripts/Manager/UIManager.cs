using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<CanvasGroup> canvasGroups = new();
    private readonly Dictionary<CanvasGroup, MonoBehaviour> controllers = new();
    private readonly Dictionary<CanvasGroup, Selectable> defaultSelections = new();
    private bool isChangingCanvas;
    private bool isQuitting;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if ( Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    // Controller가 Canvas 밖에 있어도 버튼을 기준으로 같은 UI의 Canvas를 찾는다.
    public static CanvasGroup ResolveCanvasGroup(Selectable button)
    {
        Canvas canvas = button != null ? button.GetComponentInParent<Canvas>() : null;
        if (canvas == null) return null;

        CanvasGroup group = canvas.GetComponent<CanvasGroup>();
        return group != null ? group : canvas.gameObject.AddComponent<CanvasGroup>();
    }

    public bool OverlayCanvas(CanvasGroup canvasGroup, MonoBehaviour controller = null,
        Selectable defaultSelection = null)
    {
        if (isQuitting || canvasGroup == null) return false;

        // 같은 Canvas의 중복 Controller가 입력과 선택을 덮어쓰지 못하게 한다.
        if (controllers.TryGetValue(canvasGroup, out MonoBehaviour owner)
            && owner != null && owner != controller) return false;
        if (isChangingCanvas) return owner == controller;

        if (!canvasGroups.Contains(canvasGroup))
        {
            canvasGroups.Add(canvasGroup);
        }
        if (controller != null) controllers[canvasGroup] = controller;
        defaultSelections[canvasGroup] = defaultSelection;

        SetActiveCanvas(canvasGroup);
        return true;
    }

    public void RemoveCanvas(CanvasGroup canvasGroup, MonoBehaviour controller = null)
    {
        // UI 전환을 위한 일시적인 OnDisable에서는 복귀할 항목을 유지한다.
        if (isChangingCanvas || isQuitting || ReferenceEquals(canvasGroup, null)) return;
        if (controller != null && controllers.TryGetValue(canvasGroup, out MonoBehaviour owner)
            && owner != controller) return;

        int index = canvasGroups.IndexOf(canvasGroup);
        if (index < 0) return;

        canvasGroups.RemoveAt(index);
        controllers.Remove(canvasGroup);
        defaultSelections.Remove(canvasGroup);

        if (index == canvasGroups.Count && canvasGroups.Count > 0)
        {
            SetActiveCanvas(canvasGroups[canvasGroups.Count - 1]);
        }
        else if (canvasGroups.Count == 0 && EventSystem.current != null
            && !EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void SetActiveCanvas(CanvasGroup canvasGroup)
    {
        if (isChangingCanvas || isQuitting || canvasGroup == null
            || !canvasGroups.Contains(canvasGroup)) return;

        canvasGroups.Remove(canvasGroup);
        canvasGroups.Add(canvasGroup);

        isChangingCanvas = true;
        try
        {
            // 먼저 표시와 입력을 갱신한 뒤 최상단 Canvas의 선택을 복원한다.
            foreach (CanvasGroup canvas in canvasGroups)
            {
                if (canvas == null) continue;
                bool isActive = canvas == canvasGroup;
                canvas.enabled = true;
                canvas.alpha = isActive ? 1f : 0f;
                canvas.interactable = isActive;
                canvas.blocksRaycasts = isActive;
            }

            foreach (CanvasGroup canvas in canvasGroups)
            {
                if (canvas != null && controllers.TryGetValue(canvas, out MonoBehaviour controller)
                    && controller != null)
                {
                    controller.enabled = canvas == canvasGroup;
                }
            }
        }
        finally
        {
            isChangingCanvas = false;
        }

        RestoreSelection();
    }

    private void LateUpdate()
    {
        // 언로드 후 선택 해제나 빈 공간 클릭으로 선택이 사라져도 복구한다.
        RestoreSelection();
    }

    private void RestoreSelection()
    {
        EventSystem eventSystem = EventSystem.current;
        if (isChangingCanvas || isQuitting || canvasGroups.Count == 0
            || eventSystem == null || eventSystem.alreadySelecting) return;

        CanvasGroup canvas = canvasGroups[canvasGroups.Count - 1];
        if (canvas == null || !canvas.gameObject.activeInHierarchy) return;

        GameObject selected = eventSystem.currentSelectedGameObject;
        Selectable current = selected != null ? selected.GetComponent<Selectable>() : null;
        // 현재 UI 안의 유효한 선택은 유지해 기존 Explicit 경로를 방해하지 않는다.
        if (current != null && current.IsActive() && current.transform.IsChildOf(canvas.transform)
            && current.navigation.mode != Navigation.Mode.None) return;

        defaultSelections.TryGetValue(canvas, out Selectable target);
        if (!IsSelectable(target, canvas))
        {
            target = null;
            foreach (Selectable candidate in canvas.GetComponentsInChildren<Selectable>())
            {
                if (!IsSelectable(candidate, canvas)) continue;
                target = candidate;
                break;
            }
        }

        eventSystem.SetSelectedGameObject(target != null ? target.gameObject : null);
    }

    private static bool IsSelectable(Selectable selectable, CanvasGroup canvas)
    {
        return selectable != null && selectable.transform.IsChildOf(canvas.transform)
            && selectable.IsActive() && selectable.IsInteractable()
            && selectable.navigation.mode != Navigation.Mode.None;
    }
}
