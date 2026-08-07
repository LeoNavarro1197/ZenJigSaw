using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    // Función para volver al menú
    public void BackToMenu()
    {
        // 1. Le decimos al PuzzleManager que guarde el progreso actual antes de irnos
        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.SaveCurrentProgress();
        }

        // 2. Cargamos la escena del Menú
        SceneManager.LoadScene("MainMenu");
    }
}