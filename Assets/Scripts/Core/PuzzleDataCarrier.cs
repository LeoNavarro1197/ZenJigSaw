using UnityEngine;

public static class PuzzleDataCarrier
{
    public static Texture2D selectedImage;
    public static int columns = 4;
    public static int rows = 4;

    // NUEVO: Identificador único para guardar el progreso
    public static string currentPuzzleId = "Custom";
    public static string currentPuzzleName = "Custom Puzzle";

    // Nuevas variables para fotos personalizadas
    public static bool isCustomPuzzle = false;
    public static string customImagePath = "";

    public static void SetCustomImage(Texture2D texture)
    {
        selectedImage = texture;
        currentPuzzleId = "Custom_" + texture.GetHashCode(); // ID único basado en la foto
        currentPuzzleName = "Mi Foto";
    }
}