using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;

    private AndroidJavaObject vibrator;
    private int androidApiLevel = 0;
    private bool isSupported = false;

    void Awake()
    {
        // Singleton
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // 1. Descubrimos qué versión de Android tiene el teléfono
            using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                androidApiLevel = versionClass.GetStatic<int>("SDK_INT");
            }

            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                // 2. Si es Android 12 o superior (API 31+), usamos el VibratorManager nuevo
                if (androidApiLevel >= 31)
                {
                    using (AndroidJavaObject vibratorManager = activity.Call<AndroidJavaObject>("getSystemService", "vibrator_manager"))
                    {
                        if (vibratorManager != null)
                        {
                            vibrator = vibratorManager.Call<AndroidJavaObject>("getDefaultVibrator");
                        }
                    }
                }
                // 3. Si es Android 11 o inferior, usamos el Vibrator clásico
                else
                {
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                // 4. Verificamos si el teléfono realmente tiene motor de vibración
                if (vibrator != null)
                {
                    isSupported = vibrator.Call<bool>("hasVibrator");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error inicializando el vibrador: " + e.Message);
        }
#endif
    }

    public void TriggerLightVibration()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (isSupported && vibrator != null)
        {
            try
            {
                // Si es Android 8.0 (Oreo) o superior, usamos VibrationEffect
                if (androidApiLevel >= 26)
                {
                    using (AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        int defaultAmplitude = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
                        
                        // Vibración de 25 milisegundos
                        using (AndroidJavaObject effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", 25L, defaultAmplitude))
                        {
                            if (effect != null)
                            {
                                vibrator.Call("vibrate", effect);
                            }
                        }
                    }
                }
                // Si es un Android súper viejo, usamos el método legacy
                else
                {
                    vibrator.Call("vibrate", 25L);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error al vibrar: " + e.Message);
            }
        }
#elif UNITY_IOS && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
    }
}