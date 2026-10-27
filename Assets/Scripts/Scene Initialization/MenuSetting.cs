using UnityEngine;

public class MenuSetting : MonoBehaviour
{
    private void OnEnable()
    {
        CursorManager.Instance.setCursorVisible(true);
    }

    private void OnDisable()
    {
        CursorManager.Instance.setCursorVisible(false);
    }
}
