using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

// Añadimos IBeginDragHandler para decidir qué hacer justo al empezar a arrastrar
[RequireComponent(typeof(Image))]
public class PuzzlePiece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IPointerUpHandler
{
    [HideInInspector] public Vector2 correctPosition;
    [HideInInspector] public bool isPlacedCorrectly = false;
    public float snapDistance = 50f;
    [HideInInspector] public Vector2 boardSize;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;
    private Vector2 dragOffset;

    private bool isDraggingPiece = false; // ¿Estamos arrastrando la ficha o haciendo scroll?

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;
        // Solo damos feedback visual de que la tocamos
        canvasGroup.alpha = 0.7f;
    }

    // Se llama JUSTO cuando el dedo/ratón empieza a moverse
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

        // 1. Verificamos si la ficha está actualmente en la bandeja
        bool isInTray = transform.parent == PuzzleManager.Instance.trayContent;

        // 2. Si está en la bandeja Y el movimiento es horizontal -> Hacemos Scroll
        if (isInTray && Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y))
        {
            isDraggingPiece = false;
            ExecuteEvents.ExecuteHierarchy<IBeginDragHandler>(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
        else
        {
            // 3. Si está en el tablero (o si la subimos verticalmente desde la bandeja) -> Agarramos la ficha
            isDraggingPiece = true;
            canvasGroup.blocksRaycasts = false;

            // Si estaba en la bandeja, la cambiamos al tablero y le damos su tamaño correcto
            if (isInTray)
            {
                Vector3 worldPos = transform.position;
                transform.SetParent(PuzzleManager.Instance.boardArea, true);
                rectTransform.sizeDelta = boardSize;
                transform.position = worldPos;
            }

            // Calculamos el offset (sin importar si vino de la bandeja o ya estaba en el tablero)
            if (parentCanvas == null) parentCanvas = GetComponentInParent<Canvas>();
            Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                cam,
                out Vector2 localPoint);

            dragOffset = rectTransform.anchoredPosition - localPoint;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

        if (isDraggingPiece)
        {
            // Movemos la ficha
            Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform.parent as RectTransform,
                eventData.position,
                cam,
                out Vector2 localPoint);

            rectTransform.anchoredPosition = localPoint + dragOffset;
        }
        else
        {
            // Si no estamos moviendo la ficha, le pasamos el movimiento a la bandeja (ScrollRect)
            ExecuteEvents.ExecuteHierarchy<IDragHandler>(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

        // Restauramos la opacidad
        canvasGroup.alpha = 1f;

        // Si estábamos arrastrando la ficha, comprobamos si encajó
        if (isDraggingPiece)
        {
            isDraggingPiece = false;
            canvasGroup.blocksRaycasts = true;

            float distance = Vector2.Distance(rectTransform.anchoredPosition, correctPosition);

            Debug.Log($"Distancia: {distance} | Requerida: {snapDistance} | Pos Actual: {rectTransform.anchoredPosition} | Pos Correcta: {correctPosition}");

            if (distance <= snapDistance)
            {
                isPlacedCorrectly = true;
                canvasGroup.blocksRaycasts = false;
                rectTransform.DOAnchorPos(correctPosition, 0.2f).SetEase(Ease.OutBack);
                transform.DOScale(1.1f, 0.1f).OnComplete(() => transform.DOScale(1f, 0.1f));
                PuzzleManager.Instance.PiecePlaced();
            }
        }
    }
}