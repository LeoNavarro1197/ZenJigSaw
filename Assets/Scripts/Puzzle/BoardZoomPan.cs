using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class BoardZoomPan : MonoBehaviour, IDragHandler
{
    public RectTransform boardArea;
    public float minScale = 1f;
    public float maxScale = 2.5f;

    private float currentScale = 1f;
    private float prevPinchDist = 0f;

    void OnEnable() { EnhancedTouchSupport.Enable(); }
    void OnDisable() { EnhancedTouchSupport.Disable(); }

    void Update()
    {
#if UNITY_EDITOR
        // --- ZOOM CON RUEDA DEL RATÓN ---
        if (Mouse.current != null && Mouse.current.scroll.ReadValue().y != 0)
        {
            float scroll = Mouse.current.scroll.ReadValue().y * 0.001f;
            currentScale += scroll;
            currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
            boardArea.localScale = Vector3.one * currentScale;
            ClampPosition();
        }
#endif

        // --- ZOOM CON 2 DEDOS ---
        if (Touch.activeTouches.Count == 2)
        {
            Touch t1 = Touch.activeTouches[0];
            Touch t2 = Touch.activeTouches[1];

            if (t1.phase == UnityEngine.InputSystem.TouchPhase.Began || t2.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                prevPinchDist = Vector2.Distance(t1.screenPosition, t2.screenPosition);
            }
            else if (t1.phase == UnityEngine.InputSystem.TouchPhase.Moved || t2.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                float newDist = Vector2.Distance(t1.screenPosition, t2.screenPosition);
                if (prevPinchDist > 0)
                {
                    float delta = newDist / prevPinchDist;
                    currentScale *= delta;
                    currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
                    boardArea.localScale = Vector3.one * currentScale;
                    ClampPosition();
                }
                prevPinchDist = newDist;
            }
        }

        // ¡NUEVO! EL IMÁN DEL CENTRO
        // Si el zoom es 1.0 y la posición no es (0,0), lo empujamos suavemente al centro
        if (currentScale <= 1.01f)
        {
            boardArea.anchoredPosition = Vector2.Lerp(boardArea.anchoredPosition, Vector2.zero, Time.deltaTime * 15f);

            // Si ya está muy cerca del centro, lo encajamos a la fuerza para que quede perfecto
            if (Vector2.Distance(boardArea.anchoredPosition, Vector2.zero) < 1f)
            {
                boardArea.anchoredPosition = Vector2.zero;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Solo movemos si hay 1 dedo y estamos haciendo zoom
        if (Touch.activeTouches.Count <= 1 && currentScale > 1.01f)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                boardArea.anchoredPosition += eventData.delta / canvas.scaleFactor;
                ClampPosition();
            }
        }
    }

    void ClampPosition()
    {
        if (currentScale <= 1.01f)
        {
            return; // Dejamos que el "imán" del centro se encargue en el Update
        }

        float maxPanX = (currentScale - 1f) * (boardArea.rect.width / 2f);
        float maxPanY = (currentScale - 1f) * (boardArea.rect.height / 2f);

        Vector2 pos = boardArea.anchoredPosition;

        pos.x = Mathf.Clamp(pos.x, -maxPanX, maxPanX);
        pos.y = Mathf.Clamp(pos.y, -maxPanY, maxPanY);

        boardArea.anchoredPosition = pos;
    }
}