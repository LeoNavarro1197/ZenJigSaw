using UnityEngine;

[System.Serializable] // Esto hace que la clase aparezca en el Inspector
public class PuzzleItem
{
    public string puzzleName;       // Ej. "Misty Peaks"
    public Sprite puzzleImage;      // La foto
    public int defaultPieces = 100; // Ej. 100
}

[CreateAssetMenu(fileName = "NewPuzzlePack", menuName = "ZenJigsaw/Puzzle Pack")]
public class PuzzleLevelData : ScriptableObject
{
    public string packName;             // Ej. "Naturaleza"
    public PuzzleItem[] puzzles;        // Arreglo de los rompecabezas
}