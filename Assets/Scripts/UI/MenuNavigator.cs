using UnityEngine;

public class MenuNavigator : MonoBehaviour
{
    [Header("Paneles del Menú")]
    public GameObject homeScreen;
    public GameObject customScreen;
    public GameObject collectionScreen;
    public GameObject meScreen;

    void Start()
    {
        // Al iniciar, mostramos el Home
        ShowHome();
    }

    public void ShowHome()
    {
        homeScreen.SetActive(true);
        customScreen.SetActive(false);
        collectionScreen.SetActive(false);
        meScreen.SetActive(false);
    }

    public void ShowCustom()
    {
        homeScreen.SetActive(false);
        customScreen.SetActive(true);
        collectionScreen.SetActive(false);
        meScreen.SetActive(false);
    }

    public void ShowCollection()
    {
        homeScreen.SetActive(false);
        customScreen.SetActive(false);
        collectionScreen.SetActive(true);
        meScreen.SetActive(false);
    }

    public void ShowMe()
    {
        homeScreen.SetActive(false);
        customScreen.SetActive(false);
        collectionScreen.SetActive(false);
        meScreen.SetActive(true);
    }
}