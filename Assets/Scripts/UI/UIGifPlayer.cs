using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIGifPlayer : MonoBehaviour
{
    [Header("Fotogramas del GIF")]
    public Sprite[] frames;

    [Header("Velocidad")]
    public float framesPerSecond = 10f; // Velocidad de reproducción

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        // Si hay al menos un frame, lo ponemos de inmediato
        if (frames.Length > 0)
        {
            image.sprite = frames[0];
        }
    }

    void Update()
    {
        if (frames.Length <= 0) return;

        // Calculamos qué frame tocar en este momento basado en el tiempo
        float index = Time.time * framesPerSecond;
        index = index % frames.Length; // Hacemos que se repita en bucle

        // Lo asignamos a la UI
        image.sprite = frames[Mathf.FloorToInt(index)];
    }
}