using UnityEngine;
using GoogleMobileAds.Api;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    // IDs de PRUEBA oficiales de Google (Cámbialos por los tuyos cuando vayas a publicar)
    private string bannerId = "ca-app-pub-3940256099942544/6300978111";
    private string interstitialId = "ca-app-pub-3940256099942544/1033173712";
    private string rewardedId = "ca-app-pub-3940256099942544/5224354917";

    private BannerView bannerView;
    private InterstitialAd interstitialAd;
    private RewardedAd rewardedAd;

    private System.Action onRewardedComplete;

    void Awake()
    {
        // Singleton que no se destruye al cambiar de escena
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        // Inicializamos el SDK de Google AdMob
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("Google AdMob Inicializado correctamente.");
            // ¡AQUÍ REMOVIMOS LoadBanner() para que no salga en el menú!
            LoadInterstitial();
            LoadRewardedAd();
        });
    }

    // --- BANNER ---
    void LoadBanner()
    {
        // Crear un banner abajo del todo
        bannerView = new BannerView(bannerId, AdSize.Banner, AdPosition.Bottom);

        // Cargar el anuncio
        AdRequest request = new AdRequest();
        bannerView.LoadAd(request);
    }

    // Llamaremos a esto cuando carguemos el tablero
    public void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
        else
        {
            LoadBanner(); // Si por alguna razón se destruyó, lo volvemos a crear
        }
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            // Destruimos el banner por completo para limpiar la memoria
            bannerView.Destroy();
            bannerView = null;
        }
    }

    // --- INTERSTITIAL (Pantalla completa) ---
    void LoadInterstitial()
    {
        InterstitialAd.Load(interstitialId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("Error cargando Interstitial: " + error);
                return;
            }
            interstitialAd = ad;
            Debug.Log("Interstitial cargado y listo.");
        });
    }

    public void ShowInterstitial()
    {
        if (interstitialAd != null && interstitialAd.CanShowAd())
        {
            interstitialAd.Show();
            // Cargamos el siguiente para tenerlo listo
            interstitialAd = null;
            LoadInterstitial();
        }
        else
        {
            Debug.Log("Interstitial no está listo todavía.");
        }
    }

    // --- REWARDED (Video Recompensado para las Ayudas) ---
    void LoadRewardedAd()
    {
        RewardedAd.Load(rewardedId, new AdRequest(), (ad, error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("Error cargando Rewarded: " + error);
                return;
            }
            rewardedAd = ad;
            Debug.Log("Rewarded cargado y listo.");
        });
    }

    public void ShowRewardedAd(System.Action callback)
    {
        onRewardedComplete = callback;

        if (rewardedAd != null && rewardedAd.CanShowAd())
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log("Recompensa dada: " + reward.Amount);
                onRewardedComplete?.Invoke();
                onRewardedComplete = null;

                // Cargamos el siguiente
                rewardedAd = null;
                LoadRewardedAd();
            });
        }
        else
        {
            Debug.Log("Rewarded no está listo. Recargando...");
            LoadRewardedAd(); // Intenta recargar si falla
        }
    }
}