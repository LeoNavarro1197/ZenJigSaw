using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class HomeManager : MonoBehaviour
{
    [Header("Base de Datos")]
    public PuzzleLevelData[] puzzlePacks;

    [Header("UI")]
    public Transform gridContent;
    public GameObject cardPrefab;

    void Start()
    {
        GenerateHomeGrid("All"); // Mostrar todos al iniciar
    }

    // Llamaremos a esta función desde los botones de categoría
    public void GenerateHomeGrid(string filter)
    {
        // Limpiar la cuadrícula
        foreach (Transform child in gridContent)
        {
            Destroy(child.gameObject);
        }

        foreach (PuzzleLevelData pack in puzzlePacks)
        {
            // Si el filtro es "All", mostramos todo. Si no, solo si el nombre del pack coincide
            if (filter == "All" || filter == pack.packName)
            {
                PopulateGrid(pack);
            }
        }
    }

    void PopulateGrid(PuzzleLevelData pack)
    {
        foreach (PuzzleItem item in pack.puzzles)
        {
            GameObject newCard = Instantiate(cardPrefab, gridContent);

            Image cardImage = newCard.GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.sprite = item.puzzleImage;
                cardImage.preserveAspect = false;
            }

            TextMeshProUGUI[] texts = newCard.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI statusText = null;

            foreach (TextMeshProUGUI txt in texts)
            {
                if (txt.name == "NameText") txt.text = item.puzzleName;
                if (txt.name == "PiecesText") txt.text = item.defaultPieces + " Pieces";
                if (txt.name == "StatusText") statusText = txt;
            }

            if (statusText != null)
            {
                PuzzleSaveData data = SaveSystem.LoadPuzzle(item.puzzleName);
                if (data != null)
                {
                    if (data.isCompleted)
                    {
                        statusText.text = "COMPLETED";
                        statusText.color = Color.green;
                    }
                    else
                    {
                        float percentage = ((float)data.placedPiecesIndices.Length / item.defaultPieces) * 100f;
                        statusText.text = Mathf.RoundToInt(percentage) + "%";
                        statusText.color = Color.yellow;
                    }
                }
                else
                {
                    statusText.text = "NEW";
                    statusText.color = Color.cyan;
                }
            }

            Button btn = newCard.GetComponent<Button>();
            PuzzleItem capturedItem = item;
            btn.onClick.AddListener(() => LoadDefaultPuzzle(capturedItem));
        }
    }

    void LoadDefaultPuzzle(PuzzleItem selectedItem)
    {
        if (selectedItem.puzzleImage.texture.isReadable)
        {
            PuzzleDataCarrier.SetCustomImage(selectedItem.puzzleImage.texture);
            PuzzleDataCarrier.currentPuzzleId = selectedItem.puzzleName;
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