using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance;

    private IStoreController storeController;
    private IExtensionProvider storeExtensionProvider;

    // ID del producto en la Google Play Console
    private const string REMOVE_ADS_ID = "remove_ads";

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(REMOVE_ADS_ID, ProductType.NonConsumable); // No consumible (se compra 1 vez)

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    // Función para comprar (la llamaremos desde el botón)
    public void BuyRemoveAds()
    {
        if (IsInitialized())
        {
            storeController.InitiatePurchase(REMOVE_ADS_ID);
        }
        else
        {
            Debug.Log("IAP no está inicializado todavía.");
        }
    }

    // Función para restaurar compras (la llamaremos desde el botón)
    public void RestorePurchases()
    {
        if (!IsInitialized()) return;

        // En Android usamos esta extensión
        if (Application.platform == RuntimePlatform.Android)
        {
            var android = storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            android.RestoreTransactions((bool success, string message) => {
                Debug.Log("Restauración de compras iniciada. Éxito: " + success);
                CheckIfAdsWereRemoved(); // Verificamos si ya tenía la compra
            });
        }
        else
        {
            Debug.Log("Restauración no soportada en esta plataforma.");
        }
    }

    // --- Callbacks de Unity IAP ---

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        storeExtensionProvider = extensions;
        Debug.Log("IAP Inicializado correctamente.");
        CheckIfAdsWereRemoved(); // Al arrancar, revisamos si ya lo compró
    }

    public void OnInitializeFailed(InitializationFailureReason error) { Debug.Log("Error IAP: " + error); }
    public void OnInitializeFailed(InitializationFailureReason error, string message) { Debug.Log("Error IAP: " + error + " - " + message); }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log($"Compra fallida: {product.definition.id}. Motivo: {failureReason}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (string.Equals(args.purchasedProduct.definition.id, REMOVE_ADS_ID, System.StringComparison.Ordinal))
        {
            Debug.Log("¡Compra exitosa! Quitando anuncios.");
            PlayerPrefs.SetInt("RemoveAds", 1); // Guardamos que compró
            PlayerPrefs.Save();

            // Ocultamos el banner inmediatamente si está visible
            if (AdsManager.Instance != null) AdsManager.Instance.HideBanner();
        }
        return PurchaseProcessingResult.Complete;
    }

    // Verifica si el jugador ya había comprado "Quitar Anuncios"
    private void CheckIfAdsWereRemoved()
    {
        if (storeController != null)
        {
            Product product = storeController.products.WithID(REMOVE_ADS_ID);
            if (product != null && product.hasReceipt)
            {
                PlayerPrefs.SetInt("RemoveAds", 1);
                PlayerPrefs.Save();
                if (AdsManager.Instance != null) AdsManager.Instance.HideBanner();
            }
        }
    }
}