using GoogleMobileAds.Api;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollectionManager : MonoBehaviour
{
    public PuzzleLevelData[] puzzlePacks;
    public Transform gridContent;
    public GameObject cardPrefab;
    public GameObject emptyStateText;

    [System.Serializable]
    public class CollectionEntry
    {
        public string puzzleId;
        public string puzzleName;
        public Texture2D tex;
        public Sprite sprite;
        public PuzzleSaveData data;
        public int totalPieces;
        public string imgPath;
        public bool isCustom;
        public long lastPlayed;
    }

    public void UpdateCollection()
    {
        // Limpiar la cuadrícula
        foreach (Transform child in gridContent) Destroy(child.gameObject);

        List<CollectionEntry> entries = new List<CollectionEntry>();

        // 1. Leer TODOS los archivos .json guardados en el teléfono
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] files = dir.GetFiles("*.json");

        foreach (FileInfo file in files)
        {
            string json = File.ReadAllText(file.FullName);
            PuzzleSaveData data = JsonUtility.FromJson<PuzzleSaveData>(json);

            // Ignoramos archivos viejos que no tengan timestamp
            long timestamp = data.lastPlayedTimestamp;

            string puzzleId = file.Name.Replace(".json", "");
            if (puzzleId.EndsWith("_bg")) continue; // Ignorar archivos de fondo

            CollectionEntry entry = new CollectionEntry
            {
                puzzleId = puzzleId,
                puzzleName = puzzleId,
                data = data,
                totalPieces = data.cols * data.rows,
                isCustom = puzzleId.StartsWith("Custom_"),
                imgPath = data.customImagePath,
                lastPlayed = timestamp
            };

            // 2. Buscar la imagen visual correspondiente
            if (entry.isCustom)
            {
                entry.puzzleName = "My Picture";
                if (File.Exists(data.customImagePath))
                {
                    byte[] imgBytes = File.ReadAllBytes(data.customImagePath);
                    entry.tex = new Texture2D(2, 2);
                    entry.tex.LoadImage(imgBytes);
                    entry.sprite = Sprite.Create(entry.tex, new Rect(0, 0, entry.tex.width, entry.tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            else
            {
                // Buscar en ScriptableObjects (Home)
                bool found = false;
                foreach (PuzzleLevelData pack in puzzlePacks)
                {
                    foreach (PuzzleItem item in pack.puzzles)
                    {
                        if (item.puzzleName == puzzleId)
                        {
                            entry.puzzleName = item.puzzleName;
                            entry.totalPieces = item.defaultPieces;
                            entry.sprite = item.puzzleImage;
                            entry.tex = item.puzzleImage.texture;
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }

                // Buscar en Firebase (Nube)
                if (!found)
                {
                    foreach (var dp in FirebaseManager.cachedDynamicPuzzles)
                    {
                        if (dp.name == puzzleId)
                        {
                            entry.puzzleName = dp.name;
                            entry.totalPieces = dp.pieces;
                            entry.tex = dp.tex;
                            entry.sprite = Sprite.Create(entry.tex, new Rect(0, 0, entry.tex.width, entry.tex.height), new Vector2(0.5f, 0.5f));
                            found = true;
                            break;
                        }
                    }
                }
            }

            // 3. Si encontramos la imagen, lo añadimos a la lista
            if (entry.sprite != null)
            {
                entries.Add(entry);
            }
        }

        // 4. ¡LA MAGIA! Ordenar la lista por el más reciente (de mayor a menor timestamp)
        entries.Sort((x, y) => y.lastPlayed.CompareTo(x.lastPlayed));

        // 5. Crear las tarjetas en orden
        int instantiatedCount = 0;
        foreach (var entry in entries)
        {
            GameObject newCard = Instantiate(cardPrefab, gridContent);
            SetupCard(newCard, entry);
            instantiatedCount++;
        }

        if (emptyStateText != null)
        {
            emptyStateText.SetActive(instantiatedCount == 0);
        }
    }

    void SetupCard(GameObject card, CollectionEntry entry)
    {
        Image cardImage = card.transform.Find("ImageMask/PuzzlePhoto").GetComponent<Image>();
        if (cardImage != null)
        {
            cardImage.sprite = entry.sprite;
            cardImage.preserveAspect = false;
        }

        TextMeshProUGUI[] texts = card.GetComponentsInChildren<TextMeshProUGUI>();
        TextMeshProUGUI statusText = null;

        foreach (TextMeshProUGUI txt in texts)
        {
            if (txt.name == "NameText") txt.text = entry.puzzleName;
            if (txt.name == "PiecesText")
            {
                txt.text = entry.totalPieces + " Pieces";
                //txt.color = GetDifficultyColor(entry.totalPieces);
            }
            if (txt.name == "StatusText") statusText = txt;
        }

        if (statusText != null)
        {
            if (entry.data.isCompleted)
            {
                statusText.text = "Completed";
                //statusText.color = Color.green;
            }
            else
            {
                float percentage = entry.totalPieces > 0 ? ((float)entry.data.placedPiecesIndices.Length / entry.totalPieces) * 100f : 0;
                statusText.text = Mathf.RoundToInt(percentage) + "%";
                //statusText.color = Color.yellow;
            }
        }

        Button btn = card.GetComponent<Button>();
        if (btn != null)
        {
            int safeCols = entry.data.cols > 0 ? entry.data.cols : Mathf.CeilToInt(Mathf.Sqrt(entry.totalPieces));
            int safeRows = entry.data.rows > 0 ? entry.data.rows : Mathf.CeilToInt((float)entry.totalPieces / safeCols);
            btn.onClick.AddListener(() => LoadPuzzle(entry.tex, entry.puzzleName, entry.puzzleId, safeCols, safeRows, entry.imgPath, entry.isCustom));
        }
    }

    void LoadPuzzle(Texture2D tex, string name, string puzzleId, int cols, int rows, string imgPath, bool isCustom)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        PuzzleDataCarrier.selectedImage = tex;
        PuzzleDataCarrier.currentPuzzleName = name;
        PuzzleDataCarrier.currentPuzzleId = puzzleId;
        PuzzleDataCarrier.columns = cols;
        PuzzleDataCarrier.rows = rows;

        if (isCustom)
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

    Color GetDifficultyColor(int pieces)
    {
        if (pieces <= 36) return Color.green;
        if (pieces <= 100) return Color.yellow;
        return Color.red;
    }
}