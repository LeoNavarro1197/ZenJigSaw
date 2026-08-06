using UnityEngine;

public static class ImageSlicer
{
    // Almacena las pestañas para que las fichas coincidan entre sí
    private static int[,] rightTabs;
    private static int[,] leftTabs;
    private static int[,] topTabs;
    private static int[,] bottomTabs;

    public static void InitTabs(int cols, int rows)
    {
        rightTabs = new int[cols, rows];
        leftTabs = new int[cols, rows];
        topTabs = new int[cols, rows];
        bottomTabs = new int[cols, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Bordes del tablero siempre rectos (0)
                if (c == cols - 1) rightTabs[c, r] = 0;
                else
                {
                    int t = Random.Range(0, 2) == 0 ? 1 : -1;
                    rightTabs[c, r] = t;
                    leftTabs[c + 1, r] = -t; // La ficha de la derecha necesita el hueco
                }

                if (r == rows - 1) topTabs[c, r] = 0;
                else
                {
                    int t = Random.Range(0, 2) == 0 ? 1 : -1;
                    topTabs[c, r] = t;
                    bottomTabs[c, r + 1] = -t; // La ficha de abajo necesita el hueco
                }
            }
        }
    }

    public static Sprite[] Slice(Texture2D image, int columns, int rows)
    {
        if (image == null || !image.isReadable) return null;

        int squareSize = Mathf.Min(image.width, image.height);
        int offsetX = (image.width - squareSize) / 2;
        int offsetY = (image.height - squareSize) / 2;

        int pieceSize = squareSize / columns;

        // Padding del 25% para que las pestañas circulares quepan
        int padding = pieceSize / 4;
        int texSize = pieceSize + (padding * 2);

        Sprite[] pieces = new Sprite[columns * rows];
        int index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Texture2D pieceTex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
                Color[] pixels = new Color[texSize * texSize];

                int cellStartX = offsetX + (col * pieceSize);
                int cellStartY = offsetY + (row * pieceSize);

                for (int y = 0; y < texSize; y++)
                {
                    for (int x = 0; x < texSize; x++)
                    {
                        int srcX = cellStartX - padding + x;
                        int srcY = cellStartY - padding + y;
                        srcX = Mathf.Clamp(srcX, 0, image.width - 1);
                        srcY = Mathf.Clamp(srcY, 0, image.height - 1);

                        Color userColor = image.GetPixel(srcX, srcY);
                        Color finalColor = userColor;

                        // Coordenadas UV locales (0 a 1) para esta ficha
                        float u = (float)(x - padding) / pieceSize;
                        float v = (float)(y - padding) / pieceSize;

                        // ¿Está el píxel dentro de la forma de la ficha?
                        bool isInside = IsInsidePiece(u, v, col, row, columns, rows);

                        finalColor.a = isInside ? userColor.a : 0f;
                        pixels[y * texSize + x] = finalColor;
                    }
                }

                pieceTex.SetPixels(pixels);
                pieceTex.Apply();
                // El sprite es texSize, pero el PixelsPerUnit es pieceSize para que el escalado de la UI sea correcto
                pieces[index] = Sprite.Create(pieceTex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), pieceSize);
                index++;
            }
        }
        return pieces;
    }

    private static bool IsInsidePiece(float u, float v, int col, int row, int cols, int rows)
    {
        // 1. Cuadrado base (0 a 1)
        bool inBaseSquare = (u >= -0.01f && u <= 1.01f && v >= -0.01f && v <= 1.01f);
        bool inTab = false;
        bool inBlank = false;

        // 2. Pestañas (Círculos que sobresalen)
        if (rightTabs[col, row] == 1 && Vector2.Distance(new Vector2(u, v), new Vector2(1f, 0.5f)) < 0.2f) inTab = true;
        if (leftTabs[col, row] == 1 && Vector2.Distance(new Vector2(u, v), new Vector2(0f, 0.5f)) < 0.2f) inTab = true;
        if (topTabs[col, row] == 1 && Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 1f)) < 0.2f) inTab = true;
        if (bottomTabs[col, row] == 1 && Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0f)) < 0.2f) inTab = true;

        // 3. Huecos (Círculos que se meten hacia adentro)
        if (rightTabs[col, row] == -1 && Vector2.Distance(new Vector2(u, v), new Vector2(1f, 0.5f)) < 0.2f) inBlank = true;
        if (leftTabs[col, row] == -1 && Vector2.Distance(new Vector2(u, v), new Vector2(0f, 0.5f)) < 0.2f) inBlank = true;
        if (topTabs[col, row] == -1 && Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 1f)) < 0.2f) inBlank = true;
        if (bottomTabs[col, row] == -1 && Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0f)) < 0.2f) inBlank = true;

        // Si está en un hueco, se corta. Si está en la base o en una pestaña, se queda.
        if (inBlank) return false;
        return inBaseSquare || inTab;
    }
}