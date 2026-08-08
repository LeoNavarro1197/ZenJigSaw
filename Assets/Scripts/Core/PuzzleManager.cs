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

    public GameObject loadingPanel;

    [Header("Máscaras de Fichas")]
    public Texture2D[] puzzleMasks; // Arrastraremos aquí todas tus máscaras

    [Header("Fondos de Tablero")]
    public Sprite[] availableBackgrounds; // Arrastraremos aquí las texturas de madera, corcho, etc.
    public Image boardImage;

    private const string BG_PREF_KEY = "SelectedBackgroundIndex"; // Clave para guardar

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
        StartCoroutine(StartGameRoutine());
    }

    System.Collections.IEnumerator StartGameRoutine()
    {
        // 1. Mostramos el panel de carga
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // 2. Esperamos un fotograma para que Unity dibuje el panel en pantalla
        yield return null;

        // 3. Cargamos los datos
        if (PuzzleDataCarrier.selectedImage != null)
        {
            imageToSlice = PuzzleDataCarrier.selectedImage;
            columns = PuzzleDataCarrier.columns;
            rows = PuzzleDataCarrier.rows;

            if (ghostImage != null)
            {
                Sprite previewSprite = Sprite.Create(imageToSlice, new Rect(0, 0, imageToSlice.width, imageToSlice.height), new Vector2(0.5f, 0.5f));
                ghostImage.sprite = previewSprite;
            }
        }

        // Cargar el fondo guardado por el jugador
        LoadSavedBackground();

        // 4. Generamos las fichas y cargamos el progreso (¡Solo una vez!)
        GeneratePuzzle();
        LoadSavedProgress();

        if (trayScrollRect != null)
        {
            trayScrollRect.horizontalNormalizedPosition = 0f;
        }

        // 5. Ocultamos el panel de carga
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    void GeneratePuzzle()
    {
        // ¡RED DE SEGURIDAD! Limpiamos la bandeja por si acaso se llamó dos veces
        foreach (Transform child in trayContent)
        {
            Destroy(child.gameObject);
        }
        allPieces.Clear(); // Limpiamos la lista de fichas

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

        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject newPiece = Instantiate(piecePrefab, trayContent);
            PuzzlePiece pp = newPiece.GetComponent<PuzzlePiece>();
            pp.pieceID = i; // Le damos su ID original (0 a 15, por ejemplo)
            RectTransform rt = newPiece.GetComponent<RectTransform>();
            UnityEngine.UI.Image img = newPiece.GetComponent<UnityEngine.UI.Image>();

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            img.preserveAspect = false;
            img.sprite = sprites[i];
            img.alphaHitTestMinimumThreshold = 0.1f; // Solo tocar la parte sólida

            // 1. Calculamos el tamaño visual para el tablero (1.5 veces más grande para las pestañas)
            float visualWidth = pieceWidth * 1.5f;
            float visualHeight = pieceHeight * 1.5f;

            // 2. Le decimos a la ficha cuál será su tamaño cuando vaya al tablero
            pp.boardSize = new Vector2(visualWidth, visualHeight);

            // 3. PERO en la bandeja, la instanciamos con el TAMAÑO FIJO que configuramos
            rt.sizeDelta = trayPieceSize;

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

    public void PiecePlaced()
    {
        placedPieces++;

        // ¡NUEVO: Guardar progreso automáticamente!
        SaveCurrentProgress();

        if (placedPieces >= totalPieces)
        {
            WinGame();
        }
    }

    public void SaveCurrentProgress()
    {
        System.Collections.Generic.List<int> placedIndices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < allPieces.Count; i++)
        {
            if (allPieces[i].isPlacedCorrectly)
            {
                placedIndices.Add(allPieces[i].pieceID);
            }
        }

        string imgPath = PuzzleDataCarrier.isCustomPuzzle ? PuzzleDataCarrier.customImagePath : "";

        // NUEVO: Pasamos columns y rows al guardar
        SaveSystem.SavePuzzle(PuzzleDataCarrier.currentPuzzleId, placedIndices.ToArray(), placedPieces >= totalPieces, imgPath, PuzzleDataCarrier.columns, PuzzleDataCarrier.rows);
    }

    void LoadSavedProgress()
    {
        PuzzleSaveData data = SaveSystem.LoadPuzzle(PuzzleDataCarrier.currentPuzzleId);
        if (data != null)
        {
            foreach (int savedID in data.placedPiecesIndices)
            {
                PuzzlePiece pieceToPlace = allPieces.Find(p => p.pieceID == savedID);
                if (pieceToPlace != null && !pieceToPlace.isPlacedCorrectly)
                {
                    pieceToPlace.PlaceAutomatically();
                }
            }
            if (data.isCompleted && placedPieces >= totalPieces) WinGame();
        }
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

    // Esta función la llamará el Popup cuando el jugador elija un fondo
    public void SetBoardBackground(int bgIndex)
    {
        if (availableBackgrounds.Length > 0 && bgIndex >= 0 && bgIndex < availableBackgrounds.Length)
        {
            boardImage.sprite = availableBackgrounds[bgIndex];
            boardImage.color = Color.white; // Aseguramos que no tenga tinte

            // ¡Lo guardamos en la memoria DEL TELEFONO usando el ID de este puzzle!
            string bgKey = PuzzleDataCarrier.currentPuzzleId + "_bg";
            PlayerPrefs.SetInt(bgKey, bgIndex);
            PlayerPrefs.Save();
        }
    }

    // Carga el fondo al abrir el juego
    void LoadSavedBackground()
    {
        if (availableBackgrounds.Length > 0)
        {
            // Buscamos si este puzzle específico ya tiene un fondo guardado
            string bgKey = PuzzleDataCarrier.currentPuzzleId + "_bg";
            int savedIndex = PlayerPrefs.GetInt(bgKey, 0); // Si no, usa el 0 por defecto

            boardImage.sprite = availableBackgrounds[savedIndex];
            boardImage.color = Color.white;
        }
    }

    // Esto crea un botón en el Inspector de Unity para borrar los datos
    [ContextMenu("Borrar Todos los Guardados")]
    public void ClearAllSaves()
    {
        string path = Application.persistentDataPath;
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);

        // Busca todos los archivos .json en la carpeta de guardado y los borra
        foreach (System.IO.FileInfo file in dir.GetFiles("*.json"))
        {
            file.Delete();
        }

        Debug.Log("¡Todos los guardados han sido borrados!");
    }
}