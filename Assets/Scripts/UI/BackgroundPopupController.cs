using UnityEngine;

public class BackgroundPopupController : MonoBehaviour
{
    public GameObject popupObject;

    public void OpenPopup()
    {
        // SONIDO
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        popupObject.SetActive(true);
    }

    public void ClosePopup()
    {
        // SONIDO
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        popupObject.SetActive(false);
    }

    // Llamaremos a esta función desde los botones, pasándole el número (0, 1, 2...)
    public void SelectBackground(int bgIndex)
    {
        // SONIDO
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        PuzzleManager.Instance.SetBoardBackground(bgIndex);
        ClosePopup();
    }
}