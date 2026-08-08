using UnityEngine;
using System.IO;

[System.Serializable]
public class PuzzleSaveData
{
    public int[] placedPiecesIndices;
    public bool isCompleted = false;
    public string customImagePath = "";
    public int cols = 0; // NUEVO
    public int rows = 0; // NUEVO
}

public static class SaveSystem
{
    // Guarda el progreso de un rompecabezas específico por su nombre/ID
    public static void SavePuzzle(string puzzleId, int[] placedIndices, bool isCompleted, string imagePath = "", int cols = 0, int rows = 0)
    {
        PuzzleSaveData data = new PuzzleSaveData
        {
            placedPiecesIndices = placedIndices,
            isCompleted = isCompleted,
            customImagePath = imagePath,
            cols = cols, // NUEVO
            rows = rows  // NUEVO
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, puzzleId + ".json");
        File.WriteAllText(path, json);
    }

    // Carga el progreso si existe
    public static PuzzleSaveData LoadPuzzle(string puzzleId)
    {
        string path = Path.Combine(Application.persistentDataPath, puzzleId + ".json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PuzzleSaveData>(json);
        }
        return null; // Si no hay guardado, devuelve nulo
    }
}