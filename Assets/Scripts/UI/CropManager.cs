using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CropManager : MonoBehaviour, IDragHandler
{
    public RectTransform maskArea; // El cuadrado visible
    public RawImage displayImage;  // La foto que se mueve

    private Texture2D sourceTexture;
    private Vector2 imageSizeUI;
    private Vector2 maskSizeUI;

    public void Initialize(Texture2D texture)
    {
        sourceTexture = texture;
        displayImage.texture = texture;

        maskSizeUI = maskArea.rect.size;

        // Calculamos el tamaño de la foto para que cubra el cuadrado (Cover fit)
        float aspectRatio = (float)texture.width / texture.height;
        if (aspectRatio > 1) // Horizontal
        {
            imageSizeUI = new Vector2(maskSizeUI.y * aspectRatio, maskSizeUI.y);
        }
        else // Vertical o cuadrada
        {
            imageSizeUI = new Vector2(maskSizeUI.x, maskSizeUI.x / aspectRatio);
        }

        displayImage.rectTransform.sizeDelta = imageSizeUI;
        displayImage.rectTransform.anchoredPosition = Vector2.zero; // Centrada
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Movemos la foto con el dedo/ratón
        Vector2 newPos = displayImage.rectTransform.anchoredPosition + eventData.delta;

        // Limitamos para que no se salga de los bordes del cuadrado
        float maxX = (imageSizeUI.x - maskSizeUI.x) / 2f;
        float maxY = (imageSizeUI.y - maskSizeUI.y) / 2f;

        // Si la foto es más pequeña que el mask, no la dejamos mover (maxX sería negativo)
        maxX = Mathf.Max(0, maxX);
        maxY = Mathf.Max(0, maxY);

        newPos.x = Mathf.Clamp(newPos.x, -maxX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, -maxY, maxY);

        displayImage.rectTransform.anchoredPosition = newPos;
    }

    // Esta función corta la textura y devuelve un cuadrado perfecto
    public Texture2D GetCroppedTexture()
    {
        int squareSize = Mathf.Min(sourceTexture.width, sourceTexture.height);
        Vector2 offset = displayImage.rectTransform.anchoredPosition;

        // Matemática para convertir la posición de UI a píxeles de la textura
        float normX = -offset.x / ((imageSizeUI.x - maskSizeUI.x) / 2f);
        float normY = -offset.y / ((imageSizeUI.y - maskSizeUI.y) / 2f);

        if (float.IsNaN(normX)) normX = 0;
        if (float.IsNaN(normY)) normY = 0;

        int maxOffsetX = sourceTexture.width - squareSize;
        int maxOffsetY = sourceTexture.height - squareSize;

        int startX = Mathf.RoundToInt(maxOffsetX * (normX * 0.5f + 0.5f));
        int startY = Mathf.RoundToInt(maxOffsetY * (normY * 0.5f + 0.5f));

        startX = Mathf.Clamp(startX, 0, maxOffsetX);
        startY = Mathf.Clamp(startY, 0, maxOffsetY);

        // Extraemos los píxeles y creamos la textura final
        Color[] pixels = sourceTexture.GetPixels(startX, startY, squareSize, squareSize);
        Texture2D croppedTex = new Texture2D(squareSize, squareSize, TextureFormat.RGBA32, false);
        croppedTex.SetPixels(pixels);
        croppedTex.Apply();
        return croppedTex;
    }
}