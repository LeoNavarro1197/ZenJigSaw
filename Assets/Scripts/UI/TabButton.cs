using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{
    [Header("Sprites de este botón")]
    public Sprite normalSprite;
    public Sprite selectedSprite;

    [Header("Configuración")]
    public bool selectOnStart = false;

    private Image buttonImage;

    // ¡USAMOS AWAKE EN VEZ DE START!
    void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    void Start()
    {
        // Todos empiezan con el sprite normal
        if (buttonImage != null) buttonImage.sprite = normalSprite;

        // Si este botón es el que debe estar seleccionado por defecto...
        if (selectOnStart)
        {
            SelectThisButton();
        }
    }

    public void SelectThisButton()
    {
        TabButton[] siblings = transform.parent.GetComponentsInChildren<TabButton>();

        foreach (TabButton btn in siblings)
        {
            btn.Deselect();
        }

        if (buttonImage != null) buttonImage.sprite = selectedSprite;
    }

    public void Deselect()
    {
        if (buttonImage != null) buttonImage.sprite = normalSprite;
    }
}