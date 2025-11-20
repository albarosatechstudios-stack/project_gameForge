using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DrawingController : MonoBehaviour
{
    public GameObject drawingCanvas;
    public RawImage rawImage;

    private Texture2D texture;
    private int width = 256;
    private int height = 256;

   
    private float timer = 0f;

    private int currentX = 0;
    private int currentY = 128;

    private Vector2Int? lastMousePixel = null;
    private int brushSize = 4; // dimensione pennello

    void Start()
    {
        InitTexture();
    }

    void InitTexture()
    {
        texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] whitePixels = new Color32[width * height];
        for (int i = 0; i < whitePixels.Length; i++)
            whitePixels[i] = Color.white;

        texture.SetPixels32(whitePixels);
        texture.Apply();

        rawImage.texture = texture;
    }

    void Update()
    {
       

        // Disegno con mouse destro e tratto fluido
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 localPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, mousePos, null, out localPoint))
            {
                Rect rect = rawImage.rectTransform.rect;

                float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
                float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

                int px = Mathf.Clamp((int)(normalizedX * texture.width), 0, texture.width - 1);
                int py = Mathf.Clamp((int)(normalizedY * texture.height), 0, texture.height - 1);

                Vector2Int currentPixel = new Vector2Int(px, py);

                if (lastMousePixel.HasValue)
                {
                    DrawThickLine(lastMousePixel.Value, currentPixel, brushSize, Color.black);
                }
                else
                {
                    DrawCircle(currentPixel, brushSize, Color.black);
                }

                lastMousePixel = currentPixel;

                texture.Apply();
            }
        }
        else
        {
            lastMousePixel = null;
        }

        // Pulisce la tela con il tasto C
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            ClearCanvas();
        }

        // Nasconde il pannello con ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            drawingCanvas.SetActive(false);
        }

        // Salva immagine con S
        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
        {
            string path = Application.dataPath + "/saved_drawing.png";
            SaveTextureToFile(path);
        }
    }

    void DrawAt(int x, int y, Color col)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            texture.SetPixel(x, y, col);
        }
    }

    void DrawCircle(Vector2Int center, int radius, Color col)
    {
        int rSquared = radius * radius;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= rSquared)
                {
                    int px = center.x + x;
                    int py = center.y + y;
                    DrawAt(px, py, col);
                }
            }
        }
    }

    void DrawThickLine(Vector2Int start, Vector2Int end, int thickness, Color col)
    {
        int x0 = start.x;
        int y0 = start.y;
        int x1 = end.x;
        int y1 = end.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            DrawCircle(new Vector2Int(x0, y0), thickness, col);

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    public void ClearCanvas()
    {
        Color32[] whitePixels = new Color32[width * height];
        for (int i = 0; i < whitePixels.Length; i++)
            whitePixels[i] = Color.white;

        texture.SetPixels32(whitePixels);
        texture.Apply();
    }

    public void SaveTextureToFile(string path)
    {
        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            System.IO.File.WriteAllBytes(path, pngData);
            Debug.Log($"Immagine salvata in: {path}");
        }
        else
        {
            Debug.LogError("Errore nel salvataggio PNG");
        }
    }
}
