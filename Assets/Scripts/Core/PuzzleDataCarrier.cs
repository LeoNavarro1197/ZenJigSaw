using UnityEngine;

// Esta clase estática guarda los datos entre escenas
public static class PuzzleDataCarrier
{
    public static Texture2D selectedImage;
    public static int columns = 4;
    public static int rows = 4;

    // Si la imagen viene de la galería, la convertimos a Sprite readable
    public static void SetCustomImage(Texture2D texture)
    {
        selectedImage = texture;
    }
}