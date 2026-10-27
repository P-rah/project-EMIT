using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private bool previousSetting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void setCursorVisible(bool isVisible)
    {
        Cursor.visible = isVisible;
        if(isVisible) Cursor.lockState = CursorLockMode.None;
        else Cursor.lockState = CursorLockMode.Locked;
    }

    //public void saveSetting()
    //{
    //    previousSetting = Cursor.visible;
    //}
    //public void loadSetting()
    //{
    //    setCursorVisible(previousSetting);
    //}
}
