using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeManager : MonoBehaviour
{
    [Header("Base de Datos")]
    public PuzzleLevelData[] puzzlePacks; // Arrastraremos aquí los Packs (ej. NaturePack)

    [Header("UI")]
    public Transform gridContent; // Arrastraremos aquí el Content del Scroll
    public GameObject cardPrefab;  // Arrastraremos aquí el PuzzleCard_Prefab

    void Start()
    {
        GenerateHomeGrid();
    }

    void GenerateHomeGrid()
    {
        // Limpiar la cuadrícula por si acaso
        foreach (Transform child in gridContent)
        {
            Destroy(child.gameObject);
        }

        foreach (PuzzleLevelData pack in puzzlePacks)
        {
            foreach (PuzzleItem item in pack.puzzles)
            {
                GameObject newCard = Instantiate(cardPrefab, gridContent);

                // 1. Poner la imagen
                Image cardImage = newCard.GetComponent<Image>();
                if (cardImage != null)
                {
                    cardImage.sprite = item.puzzleImage;
                    cardImage.preserveAspect = false;
                }

                // 2. Buscar los textos
                TextMeshProUGUI[] texts = newCard.GetComponentsInChildren<TextMeshProUGUI>();
                TextMeshProUGUI statusText = null;

                foreach (TextMeshProUGUI txt in texts)
                {
                    if (txt.name == "NameText") txt.text = item.puzzleName;
                    if (txt.name == "PiecesText") txt.text = item.defaultPieces + " Pieces";
                    if (txt.name == "StatusText") statusText = txt; // Guardamos la referencia del texto de estado
                }

                // 3. ¡LEER EL PROGRESO GUARDADO!
                if (statusText != null)
                {
                    // Cargamos el archivo de guardado usando el nombre del rompecabezas
                    PuzzleSaveData data = SaveSystem.LoadPuzzle(item.puzzleName);

                    if (data != null) // Si existe un guardado...
                    {
                        if (data.isCompleted)
                        {
                            statusText.text = "COMPLETED";
                            statusText.color = Color.green;
                        }
                        else
                        {
                            // Calculamos el porcentaje
                            float percentage = ((float)data.placedPiecesIndices.Length / item.defaultPieces) * 100f;
                            statusText.text = Mathf.RoundToInt(percentage) + "%";
                            statusText.color = Color.yellow;
                        }
                    }
                    else // Si no hay guardado, es nuevo
                    {
                        statusText.text = "NEW";
                        statusText.color = Color.cyan; // O el color que prefieras
                    }
                }

                // 4. Configurar el botón
                Button btn = newCard.GetComponent<Button>();
                PuzzleItem capturedItem = item;
                btn.onClick.AddListener(() => LoadDefaultPuzzle(capturedItem));
            }
        }
    }

    void LoadDefaultPuzzle(PuzzleItem selectedItem)
    {
        if (selectedItem.puzzleImage.texture.isReadable)
        {
            PuzzleDataCarrier.SetCustomImage(selectedItem.puzzleImage.texture);
            PuzzleDataCarrier.currentPuzzleId = selectedItem.puzzleName; // ¡NUEVO ID!
            PuzzleDataCarrier.currentPuzzleName = selectedItem.puzzleName;

            int totalPieces = selectedItem.defaultPieces;
            int cols = Mathf.CeilToInt(Mathf.Sqrt(totalPieces));
            int rows = Mathf.CeilToInt((float)totalPieces / cols);

            PuzzleDataCarrier.columns = cols;
            PuzzleDataCarrier.rows = rows;

            SceneManager.LoadScene("GameScene");
        }
    }
}