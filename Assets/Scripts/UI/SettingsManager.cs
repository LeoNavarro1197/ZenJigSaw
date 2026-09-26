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
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.BuyRemoveAds();
        }
        else
        {
            Debug.Log("IAPManager no encontrado.");
        }
    }

    public void RestoreAds()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.RestorePurchases();
        }
    }
}