using UnityEngine;

public class SnapTester : MonoBehaviour
{
    public RectTransform pieceToMove;
    public Vector2 targetPosition;

    void Start()
    {
        // Le decimos a la ficha cuál es su posición correcta
        PuzzlePiece pp = pieceToMove.GetComponent<PuzzlePiece>();
        pp.correctPosition = targetPosition;
    }
}
