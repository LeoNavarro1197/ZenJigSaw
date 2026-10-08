using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Necesario para buscar fichas fácilmente
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleManager : MonoBehaviour
{
    // Singleton para que las fichas puedan llamarlo
    public static PuzzleManager Instance;

    [Header("Configuración de Ayudas")]
    public int hintsRemaining = 5; // 5 ayudas iniciales

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

    [Header("UI de Ayudas")]
    public TextMeshProUGUI hintsCounterText;
    public GameObject hintAdPopup;
    public Button hintButton; // ¡NUEVO! Arrastraremos aquí el botón de Ayuda

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

        // ¡NUEVO! Bloqueamos el botón de ayuda mientras carga
        if (hintButton != null) hintButton.interactable = false;

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

        // 4. Generamos las fichas y cargamos el progreso
        GeneratePuzzle();
        StartCoroutine(LoadSavedProgress());

        // ¡NUEVO! Forzamos a Unity a calcular el ancho real de la bandeja AHORA MISMO
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(trayScrollRect.content);

        // Esperamos un fotograma para que el Scroll Rect se actualice con el nuevo ancho
        yield return null;

        // Ahora sí, lo mandamos a la izquierda
        if (trayScrollRect != null)
        {
            trayScrollRect.horizontalNormalizedPosition = 0f;
        }

        // 5. Ocultamos el panel de carga
        if (loadingPanel != null) loadingPanel.SetActive(false);

        // ¡NUEVO! Inicializar el texto de ayudas
        UpdateHintsUI();

        // ¡NUEVO! Mostrar anuncio interstitial al empezar
        if (AdsManager.Instance != null && PlayerPrefs.GetInt("HasPlayedOnce", 0) == 1)
        {
            AdsManager.Instance.ShowInterstitial();
        }

        // ¡NUEVO! Asegurarnos de que el banner de Google se vea en el tablero
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.ShowBanner();
        }

        PlayerPrefs.SetInt("HasPlayedOnce", 1); // Marcamos que ya jugó al menos una vez
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

        // Calculamos el tamaño visual (1.5 veces el tamaño matemático) que le pasamos al Slicer
        float minBoardSize = Mathf.Min(boardArea.rect.width, boardArea.rect.height);
        float pieceWidth = minBoardSize / columns;
        float visualSize = pieceWidth * 1.5f;

        Sprite[] sprites = ImageSlicer.Slice(imageToSlice, columns, rows, visualSize);

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

        // NUEVO: Obtenemos la fecha y hora actual del teléfono
        long currentTimestamp = System.DateTime.UtcNow.Ticks;

        // Se la pasamos al SaveSystem al final
        SaveSystem.SavePuzzle(PuzzleDataCarrier.currentPuzzleId, placedIndices.ToArray(), placedPieces >= totalPieces, imgPath, PuzzleDataCarrier.columns, PuzzleDataCarrier.rows, hintsRemaining, currentTimestamp);
    }

    // ¡AHORA ES UNA CORRUTINA!
    IEnumerator LoadSavedProgress()
    {
        PuzzleSaveData data = SaveSystem.LoadPuzzle(PuzzleDataCarrier.currentPuzzleId);
        if (data != null)
        {
            // 1. ¡NUEVO! Actualizamos las ayudas inmediatamente antes de mover fichas
            if (data.hintsRemaining >= 0 && !data.isCompleted)
            {
                hintsRemaining = data.hintsRemaining;
            }
            else if (data.isCompleted)
            {
                hintsRemaining = 0; // Si está completado, no tiene ayudas
            }
            UpdateHintsUI(); // Ponemos el número real en la UI al instante

            // 2. Ponemos las fichas una por una
            foreach (int savedID in data.placedPiecesIndices)
            {
                PuzzlePiece pieceToPlace = allPieces.Find(p => p.pieceID == savedID);
                if (pieceToPlace != null && !pieceToPlace.isPlacedCorrectly)
                {
                    pieceToPlace.PlaceAutomatically();
                    yield return new WaitForSeconds(0.05f);
                }
            }

            if (data.isCompleted && placedPieces >= totalPieces) WinGame();
        }
        else
        {
            UpdateHintsUI(); // Si es un puzzle nuevo, mostramos las 5 ayudas
        }

        // 3. ¡NUEVO! Cuando termina de cargar todo, desbloqueamos el botón
        if (hintButton != null) hintButton.interactable = true;
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

        // SONIDO DE VICTORIA
        if (AudioManager.Instance != null) AudioManager.Instance.PlayVictory();

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    // Función del botón "Limpiar" (Con Animación en World Space)
    public void ClearUnplacedPieces()
    {
        // 1. Desactivamos el Layout Group para que no mueva las fichas de golpe
        HorizontalLayoutGroup layout = trayContent.GetComponent<HorizontalLayoutGroup>();
        ContentSizeFitter fitter = trayContent.GetComponent<ContentSizeFitter>();
        if (layout != null) layout.enabled = false;
        if (fitter != null) fitter.enabled = false;

        // 2. Buscamos la posición WORLD (Mundo) del centro de la bandeja visible
        Vector3 trayWorldPos = trayScrollRect.transform.position;

        foreach (PuzzlePiece piece in allPieces)
        {
            if (!piece.isPlacedCorrectly)
            {
                // Si la ficha ya está en la bandeja, no la animamos
                if (piece.transform.parent == trayContent) continue;

                RectTransform rt = piece.GetComponent<RectTransform>();

                // 3. Calculamos la escala para que se encoga al tamaño de la bandeja
                float scaleMultiplier = trayPieceSize.x / rt.sizeDelta.x;
                Vector3 targetScale = rt.localScale * scaleMultiplier;

                // 4. Animamos usando DOMove (Posición Mundial) en vez de AnchorPos
                rt.DOMove(trayWorldPos, 0.3f).SetEase(Ease.InBack);
                rt.DOScale(targetScale, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    // 5. Cuando termina el vuelo, la metemos a la bandeja
                    rt.SetParent(trayContent, true);
                    rt.localScale = Vector3.one;
                    rt.sizeDelta = trayPieceSize;
                });
            }
        }

        // 6. Después de la animación, volvemos a encender el Layout Group
        StartCoroutine(RebuildTrayAfterDelay(0.35f, layout, fitter));
    }

    // Corrutina para arreglar el Layout Group después de la animación
    IEnumerator RebuildTrayAfterDelay(float delay, HorizontalLayoutGroup layout, ContentSizeFitter fitter)
    {
        yield return new WaitForSeconds(delay);

        // Encendemos el Layout Group
        if (layout != null) layout.enabled = true;
        if (fitter != null) fitter.enabled = true;

        // Forzamos a la bandeja a recalcular su tamaño y ordenar las fichas
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(trayContent);

        if (trayScrollRect != null)
        {
            trayScrollRect.horizontalNormalizedPosition = 0f;
        }
    }

    // Variables para guardar la posición del scroll
    private float savedScrollPosition = 0f;

    // Función para ocultar o mostrar todas las fichas
    public void SetPiecesVisibility(bool isVisible)
    {
        // 1. Si vamos a OCULTAR las fichas, guardamos dónde estaba el scroll
        if (!isVisible && trayScrollRect != null)
        {
            savedScrollPosition = trayScrollRect.horizontalNormalizedPosition;
        }

        // 2. Ocultamos o mostramos las fichas
        foreach (PuzzlePiece piece in allPieces)
        {
            if (!piece.isPlacedCorrectly)
            {
                piece.gameObject.SetActive(isVisible);
            }
        }

        // 3. Si vamos a MOSTRAR las fichas, esperamos un frame y restauramos el scroll
        if (isVisible && trayScrollRect != null)
        {
            StartCoroutine(RestoreScrollPosition());
        }
    }

    // Corrutina para restaurar el scroll después de que el Layout se recalcule
    IEnumerator RestoreScrollPosition()
    {
        // Esperamos un frame para que el Content Size Fitter calcule el nuevo ancho
        yield return null;

        // Forzamos a recalcular y restauramos la posición guardada
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(trayScrollRect.content);
        trayScrollRect.horizontalNormalizedPosition = savedScrollPosition;
    }

    // Función del botón de Ayuda (Hint)
    public void UseHint()
    {
        // 1. Si le quedan ayudas, las usa
        if (hintsRemaining > 0)
        {
            hintsRemaining--;
            UpdateHintsUI();

            // Iniciamos la corrutina para que suenen una por una
            StartCoroutine(PlaceHintPiecesRoutine());
        }
        // 2. Si NO le quedan ayudas, abrimos el Popup
        else
        {
            if (hintAdPopup != null) hintAdPopup.SetActive(true);
        }
    }

    // ¡NUEVA CORRUTINA! Coloca las fichas poco a poco para que el sonido no sature
    IEnumerator PlaceHintPiecesRoutine()
    {
        // 1. Calculamos cuántas fichas colocar
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

            pieceToPlace.PlaceAutomatically(); // Esto reproduce el sonido "Snap"
            unplacedPieces.RemoveAt(randomIndex);

            // Esperamos un microsegundo antes de poner la siguiente
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Función del botón "Ver Video" dentro del Popup
    public void ConfirmWatchAdForHint()
    {
        if (hintAdPopup != null) hintAdPopup.SetActive(false); // Cerramos el popup

        AdsManager.Instance.ShowRewardedAd(() =>
        {
            // El video terminó. Le damos SOLO 1 ayuda.
            hintsRemaining = 1;
            UpdateHintsUI(); // Actualizamos el texto (0 -> 1)
            UseHint(); // Y usamos esa ayuda directamente
        });
    }

    // Función del botón "No, gracias" dentro del Popup
    public void CloseHintPopup()
    {
        if (hintAdPopup != null) hintAdPopup.SetActive(false);
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

    public void UpdateHintsUI()
    {
        if (hintsCounterText != null)
        {
            hintsCounterText.text = hintsRemaining.ToString();
            // Si no le quedan ayudas, ponemos el texto en rojo
            //hintsCounterText.color = hintsRemaining > 0 ? Color.black : Color.red;
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