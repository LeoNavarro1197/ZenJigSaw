using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Fuentes de Audio")]
    public AudioSource musicSource; // Para música de fondo (opcional)
    public AudioSource sfxSource;   // Para efectos de sonido

    [Header("Efectos de Sonido (SFX)")]
    public AudioClip pickupSound;
    public AudioClip snapSound;
    public AudioClip victorySound;

    [Header("Interfaz UI")]
    public AudioClip buttonClickSound;

    void Awake()
    {
        // ¡NUEVO! Desbloqueamos los FPS a 60 para móviles
        Application.targetFrameRate = 60;

        // Singleton que no se destruye al cambiar de escena
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // Si no asignaste un AudioSource por defecto, creamos uno
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayButtonClick()
    {
        if (buttonClickSound != null) sfxSource.PlayOneShot(buttonClickSound, 0.7f);
    }

    // Funciones públicas para llamar desde otros scripts
    public void PlayPickup()
    {
        if (pickupSound != null) sfxSource.PlayOneShot(pickupSound, 0.5f); // Volumen al 50%
    }

    public void PlaySnap()
    {
        if (snapSound != null) sfxSource.PlayOneShot(snapSound, 0.8f);
    }

    public void PlayVictory()
    {
        if (victorySound != null) sfxSource.PlayOneShot(victorySound, 1f);
    }
}