using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class HomeManager : MonoBehaviour
{
    public static HomeManager Instance;

    [Header("Base de Datos")]
    public PuzzleLevelData[] puzzlePacks;

    [Header("UI")]
    public Transform gridContent;
    public GameObject cardPrefab;

    [HideInInspector] public string currentFilter = "All";

    // El color que me pediste: #4E614F
    private Color customTextColor = new Color(78f / 255f, 97f / 255f, 79f / 255f);

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateHomeGrid("All");
    }

    public void GenerateHomeGrid(string filter)
    {
        currentFilter = filter;

        // Limpiar la cuadrícula (¡TRUCO DEL DESTROY! Renombramos para que Find no los encuentre)
        foreach (Transform child in gridContent)
        {
            child.name = "Destroyed";
            Destroy(child.gameObject);
        }

        // 1. Mostrar rompecabezas locales (ScriptableObjects)
        foreach (PuzzleLevelData pack in puzzlePacks)
        {
            if (filter == "All" || filter == pack.packName)
            {
                PopulateGrid(pack);
            }
        }

        // 2. Mostrar rompecabezas de Firebase (desde la caché de RAM)
        if (FirebaseManager.cachedDynamicPuzzles.Count > 0)
        {
            foreach (var dp in FirebaseManager.cachedDynamicPuzzles)
            {
                if (filter == "All" || filter == dp.category)
                {
                    CreateDynamicCardPlaceholder(dp.name, dp.pieces, dp.category);
                    Sprite img = Sprite.Create(dp.tex, new Rect(0, 0, dp.tex.width, dp.tex.height), new Vector2(0.5f, 0.5f));
                    SetDynamicCardImage(dp.name, img, dp.tex, dp.pieces, dp.category);
                }
            }
        }
    }

    void PopulateGrid(PuzzleLevelData pack)
    {
        foreach (PuzzleItem item in pack.puzzles)
        {
            GameObject newCard = Instantiate(cardPrefab, gridContent);

            Image cardImage = newCard.transform.Find("ImageMask/PuzzlePhoto").GetComponent<Image>();
            if (cardImage != null)
            {
                cardImage.sprite = item.puzzleImage;
                cardImage.preserveAspect = false;

                Transform spinner = newCard.transform.Find("ImageMask/LoadingSpinner");
                if (spinner != null) spinner.gameObject.SetActive(false);
            }

            TextMeshProUGUI[] texts = newCard.GetComponentsInChildren<TextMeshProUGUI>();
            TextMeshProUGUI statusText = null;

            foreach (TextMeshProUGUI txt in texts)
            {
                txt.color = customTextColor; // Aplicamos tu color
                if (txt.name == "NameText") txt.text = item.puzzleName;
                if (txt.name == "PiecesText") txt.text = item.defaultPieces + " Pieces";
                if (txt.name == "StatusText") statusText = txt;
            }

            if (statusText != null)
            {
                PuzzleSaveData data = SaveSystem.LoadPuzzle(item.puzzleName);
                if (data != null)
                {
                    if (data.isCompleted) statusText.text = "Completed";
                    else
                    {
                        float percentage = ((float)data.placedPiecesIndices.Length / item.defaultPieces) * 100f;
                        statusText.text = Mathf.RoundToInt(percentage) + "%";
                    }
                }
                else statusText.text = "New";
            }

            Button btn = newCard.GetComponent<Button>();
            PuzzleItem capturedItem = item;
            btn.onClick.AddListener(() => LoadDefaultPuzzle(capturedItem));
        }
    }

    // 1. Crea la tarjeta inmediatamente con el spinner girando
    public void CreateDynamicCardPlaceholder(string name, int pieces, string category)
    {
        if (!gameObject.activeInHierarchy) return;
        if (currentFilter != "All" && currentFilter != category) return;
        if (gridContent.Find("Card_" + name) != null) return;

        GameObject newCard = Instantiate(cardPrefab, gridContent);
        newCard.name = "Card_" + name; // Nombre único

        Image cardImage = newCard.transform.Find("ImageMask/PuzzlePhoto").GetComponent<Image>();
        if (cardImage != null) cardImage.color = new Color(1, 1, 1, 0); // Invisible

        TextMeshProUGUI[] texts = newCard.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI statusText = null;

        foreach (TextMeshProUGUI txt in texts)
        {
            txt.color = customTextColor; // Aplicamos tu color
            if (txt.name == "NameText") txt.text = name;
            if (txt.name == "PiecesText") txt.text = pieces + " Pieces";
            if (txt.name == "StatusText") statusText = txt;
        }

        // ¡NUEVO! Leemos el progreso guardado para mostrar el % o "Completed"
        if (statusText != null)
        {
            // Usamos el "name" de Firebase como ID para buscar el archivo .json
            PuzzleSaveData data = SaveSystem.LoadPuzzle(name);
            if (data != null)
            {
                if (data.isCompleted)
                {
                    statusText.text = "Completed";
                }
                else
                {
                    float percentage = ((float)data.placedPiecesIndices.Length / pieces) * 100f;
                    statusText.text = Mathf.RoundToInt(percentage) + "%";
                }
            }
            else
            {
                statusText.text = "New"; // Si no hay guardado, es nuevo
            }
        }
    }

    public void SetDynamicCardImage(string name, Sprite img, Texture2D tex, int pieces, string category)
    {
        if (!gameObject.activeInHierarchy) return;
        if (currentFilter != "All" && currentFilter != category) return;

        Transform cardTransform = gridContent.Find("Card_" + name);
        if (cardTransform == null)
        {
            CreateDynamicCardPlaceholder(name, pieces, category);
            cardTransform = gridContent.Find("Card_" + name);
            if (cardTransform == null) return;
        }

        GameObject newCard = cardTransform.gameObject;

        Image cardImage = newCard.transform.Find("ImageMask/PuzzlePhoto").GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = img;
            cardImage.color = Color.white;
        }

        Transform spinner = newCard.transform.Find("ImageMask/LoadingSpinner");
        if (spinner != null) spinner.gameObject.SetActive(false);

        Button btn = newCard.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => LoadDynamicPuzzle(tex, name, pieces));
    }

    void LoadDefaultPuzzle(PuzzleItem selectedItem)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

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

    void LoadDynamicPuzzle(Texture2D tex, string name, int pieces)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        PuzzleDataCarrier.SetCustomImage(tex);
        PuzzleDataCarrier.currentPuzzleId = name;
        PuzzleDataCarrier.currentPuzzleName = name;
        PuzzleDataCarrier.isCustomPuzzle = false;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(pieces));
        int rows = Mathf.CeilToInt((float)pieces / cols);
        PuzzleDataCarrier.columns = cols;
        PuzzleDataCarrier.rows = rows;

        SceneManager.LoadScene("GameScene");
    }
}