using UnityEngine;
using UnityEngine.UI;

public class BackgroundPopupController : MonoBehaviour
{
    public GameObject popupObject; // El BackgroundPopup completo

    // Función para abrir el menú (la llamaremos desde el botón)
    public void OpenPopup()
    {
        popupObject.SetActive(true);
    }

    // Función para cerrar el menú (la llamaremos desde la X)
    public void ClosePopup()
    {
        popupObject.SetActive(false);
    }

    // Función para aplicar el fondo (la llamaremos desde los botones de texturas)
    public void SelectBackground(Sprite selectedSprite)
    {
        PuzzleManager.Instance.SetBoardBackground(selectedSprite);
        ClosePopup(); // Cerramos el popup después de elegir
    }
}
