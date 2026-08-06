using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq; // Necesario para buscar fichas fácilmente

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
    public UnityEngine.UI.Image ghostImage;

    public ScrollRect trayScrollRect; // Arrastraremos aquí el BottomTray

    [Header("Fondos de Tablero")]
    public Image boardImage; // La imagen del tablero

    [Header("Máscaras de Fichas")]
    public Texture2D[] puzzleMasks; // Arrastraremos aquí todas tus máscaras

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
        if (PuzzleDataCarrier.selectedImage != null)
        {
            imageToSlice = PuzzleDataCarrier.selectedImage;
            columns = PuzzleDataCarrier.columns;
            rows = PuzzleDataCarrier.rows;

            // --- NUEVO: Actualizar la imagen del botón OJO ---
            if (ghostImage != null)
            {
                // Convertimos la textura en un Sprite y la ponemos en la GhostImage
                Sprite previewSprite = Sprite.Create(imageToSlice, new Rect(0, 0, imageToSlice.width, imageToSlice.height), new Vector2(0.5f, 0.5f));
                ghostImage.sprite = previewSprite;
            }
        }

        GeneratePuzzle();

        if (trayScrollRect != null)
        {
            trayScrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    void GeneratePuzzle()
    {
        totalPieces = columns * rows;
        placedPieces = 0;

        // 1. Forzamos a Unity a calcular el tamaño real del tablero AHORA MISMO
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(boardArea);

        // 2. Inicializamos las pestañas matemáticas
        ImageSlicer.InitTabs(columns, rows);

        // 3. Cortamos la imagen (¡Ya no necesitamos máscara!)
        Sprite[] sprites = ImageSlicer.Slice(imageToSlice, columns, rows);

        // 4. Tamaño matemático base (Forzado a ser cuadrado perfecto)
        float minBoardSize = Mathf.Min(boardArea.rect.width, boardArea.rect.height);
        float pieceWidth = minBoardSize / columns;
        float pieceHeight = minBoardSize / rows;

        // 5. Tamaño visual (texSize es pieceSize + 50% padding, así que multiplicamos por 1.5f)
        float visualWidth = pieceWidth * 1.5f;
        float visualHeight = pieceHeight * 1.5f;

        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, trayContent);
            PuzzlePiece pp = newPiece.GetComponent<PuzzlePiece>();
            RectTransform rt = newPiece.GetComponent<RectTransform>();
            UnityEngine.UI.Image img = newPiece.GetComponent<UnityEngine.UI.Image>();

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            img.preserveAspect = false;
            img.sprite = sprites[i];
            img.alphaHitTestMinimumThreshold = 0.1f; // Solo tocar la parte sólida

            // Asignamos el tamaño visual
            // Asignamos el tamaño FIJO para la bandeja
            rt.sizeDelta = trayPieceSize;

            // Pero le decimos que cuando vaya al tablero, se encoga al tamaño matemático
            pp.boardSize = new Vector2(visualWidth, visualHeight);

            int col = i % columns;
            int row = i / columns;

            // Posición matemática para el encaje (El centro es el mismo)
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

    // Función para ocultar o mostrar todas las fichas
    public void SetPiecesVisibility(bool isVisible)
    {
        foreach (PuzzlePiece piece in allPieces)
        {
            // Si la ficha ya fue colocada correctamente, no la ocultamos
            if (!piece.isPlacedCorrectly)
            {
                piece.gameObject.SetActive(isVisible);
            }
        }
    }

    // Función del botón de Ayuda (Hint)
    public void UseHint()
    {
        // 1. Calculamos cuántas fichas colocar según el total (basado en nuestro GDD)
        int piecesToPlace = 1;
        if (totalPieces >= 225) piecesToPlace = 10;
        else if (totalPieces >= 144) piecesToPlace = 6;
        else if (totalPieces >= 100) piecesToPlace = 4;
        else if (totalPieces >= 64) piecesToPlace = 3;
        else if (totalPieces >= 36) piecesToPlace = 2;

        // 2. Buscamos todas las fichas que aún NO han sido colocadas
        List<PuzzlePiece> unplacedPieces = allPieces.Where(p => !p.isPlacedCorrectly).ToList();

        // 3. Colocamos aleatoriamente la cantidad calculada (o las que queden)
        for (int i = 0; i < piecesToPlace && unplacedPieces.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, unplacedPieces.Count);
            PuzzlePiece pieceToPlace = unplacedPieces[randomIndex];

            // La colocamos y la quitamos de la lista de pendientes
            pieceToPlace.PlaceAutomatically();
            unplacedPieces.RemoveAt(randomIndex);
        }
    }

    // Nueva función para cambiar la textura
    public void SetBoardBackground(Sprite newBg)
    {
        if (boardImage != null)
        {
            boardImage.sprite = newBg;
            boardImage.color = Color.white; // Aseguramos que no tenga tintes de color
        }
    }
}
