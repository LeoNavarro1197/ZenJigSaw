using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NativeGalleryNamespace;

public class CustomPuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startPuzzleButton;
    public CropManager cropManager; // <-- Arrastraremos aquí el PreviewArea

    [Header("Difficulty Settings")]
    public int[] difficultyOptions = { 16, 36, 64, 100, 144, 225 };
    private int selectedDifficulty = 64;

    void Start()
    {
        if (startPuzzleButton != null)
        {
            startPuzzleButton.gameObject.SetActive(false);
            startPuzzleButton.onClick.AddListener(StartGame);
        }
    }

    public void SelectDifficulty(int difficultyIndex)
    {
        selectedDifficulty = difficultyOptions[difficultyIndex];
    }

    public void OpenGallery()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);

                if (texture == null) return;

                // Inicializamos el recortador visual con la foto
                cropManager.Initialize(texture);

                // Activamos el botón de empezar
                startPuzzleButton.gameObject.SetActive(true);
            }
        }, "Selecciona una foto para tu rompecabezas", "image/*");
    }

    public void StartGame()
    {
        // 1. Obtenemos la foto ya recortada en cuadrado perfecto
        Texture2D finalCroppedImage = cropManager.GetCroppedTexture();

        // 2. La enviamos al juego
        PuzzleDataCarrier.SetCustomImage(finalCroppedImage);

        int totalPieces = selectedDifficulty;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalPieces));
        int rows = Mathf.CeilToInt((float)totalPieces / cols);
        PuzzleDataCarrier.columns = cols;
        PuzzleDataCarrier.rows = rows;

        // 3. Cargamos la escena
        SceneManager.LoadScene("GameScene");
    }
}