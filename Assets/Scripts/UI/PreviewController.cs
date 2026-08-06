using UnityEngine;
using UnityEngine.EventSystems;

public class PreviewController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public GameObject ghostImage;

    // Al tocar y mantener el botón
    public void OnPointerDown(PointerEventData eventData)
    {
        ghostImage.SetActive(true); // Muestra la imagen
        PuzzleManager.Instance.SetPiecesVisibility(false); // Oculta las fichas sueltas
    }

    // Al soltar el botón
    public void OnPointerUp(PointerEventData eventData)
    {
        ghostImage.SetActive(false); // Oculta la imagen
        PuzzleManager.Instance.SetPiecesVisibility(true); // Muestra las fichas de nuevo
    }
}
