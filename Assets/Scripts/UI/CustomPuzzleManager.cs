using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomPuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startPuzzleButton;
    public CropManager cropManager;

    [Header("Difficulty Settings")]
    public int[] difficultyOptions = { 16, 36, 64, 100, 144, 225 };
    private int selectedDifficulty = 64;

    void Start()
    {
        if (startPuzzleButton != null)
        {
            // 1. El botón ESTÁ VISIBLE, pero DESACTIVADO (en gris)
            startPuzzleButton.gameObject.SetActive(true);
            startPuzzleButton.interactable = false;
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

                // 2. ¡El jugador ya subió la foto! ACTIVAMOS el botón
                startPuzzleButton.interactable = true;
            }
        }, "Selecciona una foto para tu rompecabezas", "image/*");
    }

    public void StartGame()
    {
        // 1. Obtenemos la foto ya recortada
        Texture2D finalCroppedImage = cropManager.GetCroppedTexture();

        // 2. La guardamos en el teléfono como JPG
        byte[] jpgBytes = finalCroppedImage.EncodeToJPG();
        string customId = "Custom_" + System.DateTime.Now.Ticks;
        string imgPath = System.IO.Path.Combine(Application.persistentDataPath, customId + ".jpg");
        System.IO.File.WriteAllBytes(imgPath, jpgBytes);

        // 3. Enviamos los datos al juego
        PuzzleDataCarrier.SetCustomImage(finalCroppedImage);
        PuzzleDataCarrier.currentPuzzleId = customId;
        PuzzleDataCarrier.currentPuzzleName = "Mi Foto";
        PuzzleDataCarrier.isCustomPuzzle = true;
        PuzzleDataCarrier.customImagePath = imgPath;

        // 4. Calculamos la dificultad
        int totalPieces = selectedDifficulty;
        int cols = Mathf.CeilToInt(Mathf.Sqrt(totalPieces));
        int rows = Mathf.CeilToInt((float)totalPieces / cols);
        PuzzleDataCarrier.columns = cols;
        PuzzleDataCarrier.rows = rows;

        // 5. Cargamos la escena
        SceneManager.LoadScene("GameScene");
    }
}