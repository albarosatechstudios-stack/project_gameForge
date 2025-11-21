using UnityEngine;

public class SimpleLineComparer : MonoBehaviour
{
    [Header("Immagini da confrontare")]
    public Texture2D imageReference;  // la Gioconda (contorni)
    public Texture2D imagePlayer;     // disegno Paint (contorni)

    [Header("Parametri binarizzazione")]
    [Range(0, 1)] public float threshold = 0.5f; // valore soglia per binarizzare

    void Start()
    {
        if (imageReference == null || imagePlayer == null)
        {
            Debug.LogError("Assegna entrambe le immagini nel Inspector!");
            return;
        }

        float similarity = CompareLineDrawings(imageReference, imagePlayer, threshold);
        Debug.Log($"Similarità (Jaccard Index): {similarity * 100f:F2}%");
    }

    float CompareLineDrawings(Texture2D refTex, Texture2D playerTex, float binThreshold)
    {
        // Ridimensiona playerTex alle dimensioni di refTex
        Texture2D playerResized = ResizeTexture(playerTex, refTex.width, refTex.height);

        // Converte entrambe in array binari (true = linea, false = sfondo)
        bool[] refBinary = TextureToBinaryArray(refTex, binThreshold);
        bool[] playerBinary = TextureToBinaryArray(playerResized, binThreshold);

        int intersection = 0;
        int unionCount = 0;

        for (int i = 0; i < refBinary.Length; i++)
        {
            if (refBinary[i] || playerBinary[i])
            {
                unionCount++;
                if (refBinary[i] && playerBinary[i])
                    intersection++;
            }
        }

        if (unionCount == 0)
            return 0f;

        return (float)intersection / unionCount;  // Jaccard Index
    }

    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    bool[] TextureToBinaryArray(Texture2D tex, float threshold)
    {
        Color[] pixels = tex.GetPixels();
        bool[] binary = new bool[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            // Converti a grigio
            float gray = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            // Binarizza (linea se scuro)
            binary[i] = gray < threshold;
        }

        return binary;
    }
}
