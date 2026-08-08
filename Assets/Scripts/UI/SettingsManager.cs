using UnityEngine;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public TextMeshProUGUI versionText;
    public GameObject privacyPopup;

    void Start()
    {
        // Muestra la versión actual del juego
        if (versionText != null)
        {
            versionText.text = "Versión " + Application.version;
        }
    }

    public void OpenPrivacyPolicy()
    {
        privacyPopup.SetActive(true);
    }

    public void ClosePrivacyPolicy()
    {
        privacyPopup.SetActive(false);
    }

    public void RemoveAds()
    {
        // Esto lo programaremos en la Fase 5 con Unity IAP
        Debug.Log("Comprando Quitar Anuncios...");
    }

    public void RestoreAds()
    {
        // Esto lo programaremos en la Fase 5 con Unity IAP
        Debug.Log("Restaurando Compras...");
    }
}