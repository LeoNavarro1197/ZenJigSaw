using UnityEngine;

public static class ImageSlicer
{
    // Método estático al que le pasamos la textura y el número de columnas y filas
    public static Sprite[] Slice(Texture2D image, int columns, int rows)
    {
        // Verificamos que la imagen se pueda leer en la CPU
        if (image == null)
        {
            Debug.LogError("La imagen es nula.");
            return null;
        }

        // Asegurarnos de que la textura sea legible (importante para fotos de la galería)
        if (!image.isReadable)
        {
            Debug.LogError("La textura no es legible. Marca 'Read/Write Enabled' en los Import Settings.");
            return null;
        }

        // Calculamos el tamaño en píxeles de cada pieza
        int pieceWidth = image.width / columns;
        int pieceHeight = image.height / rows;

        // Total de piezas
        Sprite[] pieces = new Sprite[columns * rows];
        int index = 0;

        // Recorremos la imagen de abajo hacia arriba (las coordenadas de textura en Unity empiezan abajo)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculamos la posición X e Y de la pieza actual en la textura original
                int x = col * pieceWidth;
                int y = row * pieceHeight;

                // Creamos el Sprite recortando esa porción de la imagen
                // El "pivot" es el centro de la pieza (0.5, 0.5)
                // El "pixelsPerUnit" lo ponemos igual al ancho de la pieza para que ocupen 1 unidad en el mundo 3D/2D
                pieces[index] = Sprite.Create(image, new Rect(x, y, pieceWidth, pieceHeight), new Vector2(0.5f, 0.5f), pieceWidth);

                index++;
            }
        }

        return pieces;
    }
}
