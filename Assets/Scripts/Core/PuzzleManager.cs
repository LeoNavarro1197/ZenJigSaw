using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    // Singleton para que las fichas puedan llamarlo
    public static PuzzleManager Instance;

    [Header("Configuración del Tablero")]
    public Texture2D imageToSlice;
    public int columns = 4;
    public int rows = 4;

    [Header("Configuración de Bandeja")]
    public Vector2 trayPieceSize = new Vector2(100, 100); // Tamaño fijo en la bandeja

    [Header("Referencias de UI")]
    public RectTransform boardArea; // El área donde se arma el rompecabezas
    private List<PuzzlePiece> allPieces = new List<PuzzlePiece>(); // Lista para guardar las fichas
    public RectTransform trayContent;
    public GameObject piecePrefab;
    public GameObject winPanel; // Un panel que dirá "¡Ganaste!"

    public ScrollRect trayScrollRect; // Arrastraremos aquí el BottomTray

    private int totalPieces;
    private int placedPieces;

    void Awake()
    {
        // Configurar el Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GeneratePuzzle();

        // Forzamos a que el scroll empiece en el lado izquierdo (0 = izquierda, 1 = derecha)
        if (trayScrollRect != null)
        {
            trayScrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    void GeneratePuzzle()
    {
        totalPieces = columns * rows;
        placedPieces = 0;

        Sprite[] sprites = ImageSlicer.Slice(imageToSlice, columns, rows);

        float pieceWidth = boardArea.rect.width / columns;
        float pieceHeight = boardArea.rect.height / rows;

        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, trayContent);
            PuzzlePiece pp = newPiece.GetComponent<PuzzlePiece>();
            RectTransform rt = newPiece.GetComponent<RectTransform>();
            Image img = newPiece.GetComponent<Image>();

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            img.preserveAspect = false;
            img.sprite = sprites[i];

            // ¡AQUÍ ESTÁ LA MAGIA! 
            // Le decimos a la ficha cuál será su tamaño cuando esté en el tablero
            pp.boardSize = new Vector2(pieceWidth, pieceHeight);

            // Pero la instanciamos en la bandeja con el tamaño fijo
            rt.sizeDelta = trayPieceSize;

            int col = i % columns;
            int row = i / columns;

            float xPos = (col * pieceWidth) + (pieceWidth / 2);
            int invertedRow = (rows - 1) - row;
            float yPos = -((invertedRow * pieceHeight) + (pieceHeight / 2));

            pp.correctPosition = new Vector2(xPos, yPos);
            pp.snapDistance = pieceWidth * 0.8f;

            allPieces.Add(pp);
        }

        ShuffleTrayPieces();
    }

    // Método para mezclar el orden visual de las fichas en la bandeja
    void ShuffleTrayPieces()
    {
        // El Horizontal Layout Group ordena las fichas por su "Sibling Index" (su lugar en la jerarquía)
        // Vamos a poner las fichas en un orden aleatorio
        for (int i = 0; i < allPieces.Count; i++)
        {
            PuzzlePiece temp = allPieces[i];
            int randomIndex = Random.Range(i, allPieces.Count);
            allPieces[i] = allPieces[randomIndex];
            allPieces[randomIndex] = temp;
        }

        // Ahora aplicamos ese nuevo orden a la jerarquía visual
        for (int i = 0; i < allPieces.Count; i++)
        {
            allPieces[i].transform.SetSiblingIndex(i);
        }
    }

    // Este método lo llama la ficha cuando encaja
    public void PiecePlaced()
    {
        placedPieces++;

        // Si todas las fichas están puestas
        if (placedPieces >= totalPieces)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        Debug.Log("¡ROMPECABEZAS COMPLETADO!");
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    // Función del botón "Limpiar"
    public void ClearUnplacedPieces()
    {
        foreach (PuzzlePiece piece in allPieces)
        {
            if (!piece.isPlacedCorrectly)
            {
                piece.transform.SetParent(trayContent);
                piece.transform.SetAsFirstSibling();
                // Devolvemos su tamaño al tamaño fijo de la bandeja
                piece.GetComponent<RectTransform>().sizeDelta = trayPieceSize;
            }
        }
    }
}
