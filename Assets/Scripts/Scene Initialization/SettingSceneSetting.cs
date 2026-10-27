using UnityEngine;

public class SettingSceneSetting : MonoBehaviour
{
    private void OnEnable()
    {
        CursorManager.Instance.setCursorVisible(true);
    }

    private void OnDisable()
    {
    }
}
