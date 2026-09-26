using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.Networking;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    public static List<DynamicPuzzleData> cachedDynamicPuzzles = new List<DynamicPuzzleData>();

    [System.Serializable]
    public class DynamicPuzzleData
    {
        public string name;
        public int pieces;
        public string category;
        public Texture2D tex;
    }

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                LoadDynamicPuzzles();
            }
        });
    }

    void LoadDynamicPuzzles()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        Query query = db.Collection("puzzles");

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted) return;

            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                string name = document.GetValue<string>("name");
                int pieces = document.GetValue<int>("pieces");
                string imageUrl = document.GetValue<string>("imageUrl");
                string category = document.ContainsField("category") ? document.GetValue<string>("category") : "All";

                if (HomeManager.Instance != null)
                {
                    HomeManager.Instance.CreateDynamicCardPlaceholder(name, pieces, category);
                }

                StartCoroutine(DownloadImage(name, pieces, imageUrl, category));
            }
        });
    }

    IEnumerator DownloadImage(string name, int pieces, string imageUrl, string category)
    {
        // ¡TRUCO! Esperamos 0.5 segundos para que el spinner se vea incluso si carga de la caché local
        yield return new WaitForSeconds(0.5f);

        string localPath = System.IO.Path.Combine(Application.persistentDataPath, name + ".jpg");
        Texture2D tex = new Texture2D(2, 2);

        if (System.IO.File.Exists(localPath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(localPath);
            tex.LoadImage(fileData);
        }
        else
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                tex = DownloadHandlerTexture.GetContent(request);
                byte[] bytes = tex.EncodeToJPG();
                System.IO.File.WriteAllBytes(localPath, bytes);
            }
            else
            {
                yield break; // Si falla, dejamos el spinner girando
            }
        }

        DynamicPuzzleData data = new DynamicPuzzleData
        {
            name = name,
            pieces = pieces,
            category = category,
            tex = tex
        };
        cachedDynamicPuzzles.Add(data);

        if (HomeManager.Instance != null)
        {
            Sprite img = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            HomeManager.Instance.SetDynamicCardImage(name, img, tex, pieces, category);
        }
    }
}