using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    // Función para volver al menú
    public void BackToMenu()
    {
        // 1. SONIDO Y VIBRACIÓN AL PULSAR ATRÁS
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
        if (HapticManager.Instance != null) HapticManager.Instance.TriggerLightVibration();

        // 2. OCULTAMOS EL BANNER AL SALIR DEL JUEGO
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.HideBanner();
        }

        // 3. Guardamos el progreso actual antes de irnos
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.SaveCurrentProgress();
        }

        // 4. Cargamos la escena del Menú
        SceneManager.LoadScene("MainMenu");
    }
}