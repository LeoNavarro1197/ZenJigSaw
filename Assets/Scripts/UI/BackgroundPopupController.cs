using UnityEngine;

public class BackgroundPopupController : MonoBehaviour
{
    public GameObject popupObject;

    public void OpenPopup()
    {
        popupObject.SetActive(true);
    }

    public void ClosePopup()
    {
        popupObject.SetActive(false);
    }

    // Llamaremos a esta función desde los botones, pasándole el número (0, 1, 2...)
    public void SelectBackground(int bgIndex)
    {
        PuzzleManager.Instance.SetBoardBackground(bgIndex);
        ClosePopup();
    }
}