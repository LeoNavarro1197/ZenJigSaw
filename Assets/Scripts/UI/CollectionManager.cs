using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

public class CollectionManager : MonoBehaviour
{
    public PuzzleLevelData[] puzzlePacks;
    public Transform gridContent;
    public GameObject cardPrefab;

    [Header("UI de Estado")]
    public GameObject emptyStateText; // Arrastraremos aquí el texto de "Vacío"

    public void UpdateCollection()
    {
        // Limpiar la cuadrícula
        foreach (Transform child in gridContent) Destroy(child.gameObject);

        int instantiatedCount = 0; // Contador para saber si hay algo

        // 1. Revisar los rompecabezas predeterminados
        foreach (PuzzleLevelData pack in puzzlePacks)
        {
            foreach (PuzzleItem item in pack.puzzles)
            {
                PuzzleSaveData data = SaveSystem.LoadPuzzle(item.puzzleName);
                if (data != null) // Si existe guardado
                {
                    GameObject newCard = Instantiate(cardPrefab, gridContent);
                    SetupCard(newCard, item.puzzleImage, item.puzzleName, item.defaultPieces, data, item.puzzleImage.texture, item.puzzleName, "");
                    instantiatedCount++;
                }
            }
        }

        // 2. Revisar las fotos personalizadas
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] files = dir.GetFiles("*.json");

        foreach (FileInfo file in files)
        {
            if (file.Name.StartsWith("Custom_"))
            {
                string json = File.ReadAllText(file.FullName);
                PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);

                if (data.customImagePath != "" && File.Exists(data.customImagePath))
                {
                    byte[] imgBytes = File.ReadAllBytes(data.customImagePath);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(imgBytes);

                    Sprite customSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                    string puzzleId = file.Name.Replace(".json", "");
                    int totalPieces = data.cols * data.rows;
                    // Seguridad anti-cero por si el guardado es viejo
                    if (totalPieces <= 0) totalPieces = data.placedPiecesIndices.Length;

                    GameObject newCard = Instantiate(cardPrefab, gridContent);
                    SetupCard(newCard, customSprite, "Mi Foto", totalPieces, data, tex, puzzleId, data.customImagePath);
                    instantiatedCount++;
                }
            }
        }

        // 3. Mostrar u ocultar el texto de "Vacío"
        if (emptyStateText != null)
        {
            emptyStateText.SetActive(instantiatedCount == 0);
        }
    }

    void SetupCard(GameObject card, Sprite img, string name, int totalPieces, PuzzleSaveData data, Texture2D tex, string puzzleId, string imgPath)
    {
        Image cardImage = card.GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = img;
            cardImage.preserveAspect = false;
        }

        TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI statusText = null;

        foreach (TextMeshProUGUI txt in texts)
        {
            if (txt.name == "NameText") txt.text = name;
            if (txt.name == "PiecesText")
            {
                txt.text = totalPieces + " Pieces";
                txt.color = GetDifficultyColor(totalPieces); // ¡Color por dificultad!
            }
            if (txt.name == "StatusText") statusText = txt;
        }

        if (statusText != null)
        {
            if (data.isCompleted)
            {
                statusText.text = "COMPLETED";
                statusText.color = Color.green;
            }
            else
            {
                float percentage = totalPieces > 0 ? ((float)data.placedPiecesIndices.Length / totalPieces) * 100f : 0;
                statusText.text = Mathf.RoundToInt(percentage) + "%";
                statusText.color = Color.yellow;
            }
        }

        Button btn = card.GetComponent<Button>();
        if (btn != null)
        {
            int safeCols = data.cols > 0 ? data.cols : Mathf.CeilToInt(Mathf.Sqrt(totalPieces));
            int safeRows = data.rows > 0 ? data.rows : Mathf.CeilToInt((float)totalPieces / safeCols);
            btn.onClick.AddListener(() => LoadPuzzle(tex, name, puzzleId, safeCols, safeRows, imgPath));
        }
    }

    void LoadPuzzle(Texture2D tex, string name, string puzzleId, int cols, int rows, string imgPath)
    {
        PuzzleDataCarrier.selectedImage = tex;
        PuzzleDataCarrier.currentPuzzleName = name;
        PuzzleDataCarrier.currentPuzzleId = puzzleId;
        PuzzleDataCarrier.columns = cols;
        PuzzleDataCarrier.rows = rows;

        if (puzzleId.StartsWith("Custom_"))
        {
            PuzzleDataCarrier.isCustomPuzzle = true;
            PuzzleDataCarrier.customImagePath = imgPath;
        }
        else
        {
            PuzzleDataCarrier.isCustomPuzzle = false;
        }

        SceneManager.LoadScene("GameScene");
    }

    // Función para pintar el texto de piezas según la dificultad
    Color GetDifficultyColor(int pieces)
    {
        if (pieces <= 36) return Color.green;      // Fácil (Verde)
        if (pieces <= 100) return Color.yellow;    // Medio (Amarillo)
        return Color.red;                          // Difícil (Rojo)
    }
}