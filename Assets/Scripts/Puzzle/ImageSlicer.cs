using UnityEngine;

public static class ImageSlicer
{
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
                if (c == cols - 1) rightTabs[c, r] = 0;
                else
                {
                    int t = Random.Range(0, 2) == 0 ? 1 : -1;
                    rightTabs[c, r] = t;
                    leftTabs[c + 1, r] = -t;
                }

                if (r == rows - 1) topTabs[c, r] = 0;
                else
                {
                    int t = Random.Range(0, 2) == 0 ? 1 : -1;
                    topTabs[c, r] = t;
                    bottomTabs[c, r + 1] = -t;
                }
            }
        }
    }

    public static Sprite[] Slice(Texture2D image, int columns, int rows, float visualSize)
    {
        if (image == null || !image.isReadable) return null;

        int squareSize = Mathf.Min(image.width, image.height);
        int offsetX = (image.width - squareSize) / 2;
        int offsetY = (image.height - squareSize) / 2;

        int pieceSize = squareSize / columns;
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
                        int srcX = Mathf.Clamp(cellStartX - padding + x, 0, image.width - 1);
                        int srcY = Mathf.Clamp(cellStartY - padding + y, 0, image.height - 1);

                        Color userColor = image.GetPixel(srcX, srcY);

                        float u = (float)(x - padding) / pieceSize;
                        float v = (float)(y - padding) / pieceSize;

                        float coverage = GetPieceCoverage(u, v, col, row, columns, rows, pieceSize);

                        Color finalColor = userColor;
                        finalColor.a = coverage * userColor.a;

                        pixels[y * texSize + x] = finalColor;
                    }
                }

                pieceTex.filterMode = FilterMode.Bilinear;
                pieceTex.SetPixels(pixels);
                pieceTex.Apply();

                float pixelsPerUnit = texSize / visualSize;
                pieces[index] = Sprite.Create(pieceTex, new Rect(0, 0, texSize, texSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                index++;
            }
        }
        return pieces;
    }

    private static float GetPieceCoverage(float u, float v, int col, int row, int cols, int rows, int pieceSize)
    {
        float edge = Mathf.Max(1.5f / pieceSize, 0.015f);

        // ¡LA MAGIA AQUÍ! La pestaña es más grande que el hueco para que no se vea el fondo
        float tabR = 0.2f;
        float blankR = tabR - (edge * 1.5f);

        // 1. Base del cuadrado (con bordes suavizados)
        float sqAlpha = 1f;
        sqAlpha *= Mathf.Clamp01((u + edge) / edge);
        sqAlpha *= Mathf.Clamp01((1 + edge - u) / edge);
        sqAlpha *= Mathf.Clamp01((v + edge) / edge);
        sqAlpha *= Mathf.Clamp01((1 + edge - v) / edge);

        // 2. Pestañas (Unión - Usamos tabR que es más grande)
        float tabAlpha = 0f;
        if (rightTabs[col, row] == 1) tabAlpha = Mathf.Max(tabAlpha, CircleCoverage(u, v, 1f, 0.5f, tabR, edge));
        if (leftTabs[col, row] == 1) tabAlpha = Mathf.Max(tabAlpha, CircleCoverage(u, v, 0f, 0.5f, tabR, edge));
        if (topTabs[col, row] == 1) tabAlpha = Mathf.Max(tabAlpha, CircleCoverage(u, v, 0.5f, 1f, tabR, edge));
        if (bottomTabs[col, row] == 1) tabAlpha = Mathf.Max(tabAlpha, CircleCoverage(u, v, 0.5f, 0f, tabR, edge));

        float visibleAlpha = Mathf.Max(sqAlpha, tabAlpha);

        // 3. Huecos (Intersección - Usamos blankR que es más pequeño)
        float blankAlpha = 1f;
        if (rightTabs[col, row] == -1) blankAlpha *= 1f - CircleCoverage(u, v, 1f, 0.5f, blankR, edge);
        if (leftTabs[col, row] == -1) blankAlpha *= 1f - CircleCoverage(u, v, 0f, 0.5f, blankR, edge);
        if (topTabs[col, row] == -1) blankAlpha *= 1f - CircleCoverage(u, v, 0.5f, 1f, blankR, edge);
        if (bottomTabs[col, row] == -1) blankAlpha *= 1f - CircleCoverage(u, v, 0.5f, 0f, blankR, edge);

        float alpha = visibleAlpha * blankAlpha;

        return Mathf.Clamp01(alpha);
    }

    private static float CircleCoverage(float u, float v, float cx, float cy, float radius, float edge)
    {
        float d = Vector2.Distance(new Vector2(u, v), new Vector2(cx, cy));
        return Mathf.Clamp01((radius + edge - d) / edge);
    }
}