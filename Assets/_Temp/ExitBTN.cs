using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitBTN : MonoBehaviour
{
    public void OnClick()
    {
        SceneControlManager.Instance.LoadSettings();
    }
}
