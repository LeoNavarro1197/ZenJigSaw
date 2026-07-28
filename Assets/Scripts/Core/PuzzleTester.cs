using UnityEngine;

public class PuzzleTester : MonoBehaviour
{
    public Texture2D testImage;
    public int columns = 4;
    public int rows = 4;

    void Start()
    {
        // Cortamos la imagen
        Sprite[] pieces = ImageSlicer.Slice(testImage, columns, rows);

        if (pieces == null) return;

        // Instanciamos las piezas en la escena para verlas
        for (int i = 0; i < pieces.Length; i++)
        {
            // Creamos un GameObject nuevo por cada pieza
            GameObject obj = new GameObject("Piece_" + i);

            // Le añadimos un SpriteRenderer para verlo en pantalla
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = pieces[i];

            // Las ordenamos en fila para verlas todas
            obj.transform.position = new Vector3(i * 1.5f, 0, 0);
        }
    }
}
