using UnityEngine;
using System.IO;

public class SimpleLineComparerIgnoreBG : MonoBehaviour
{
    [Header("Immagini da confrontare")]
    public Texture2D imageReference;  // la Gioconda (contorni)
    public Texture2D imagePlayer;     // disegno Paint (contorni)

    [Header("Parametri binarizzazione")]
    [Range(0, 1)] public float thresholdLine = 0.5f;  // soglia per linee (pixel scuri)
    [Range(0, 1)] public float thresholdBG = 0.8f;    // soglia per considerare sfondo (pixel chiari)

    void Start()
    {
        if (imageReference == null || imagePlayer == null)
        {
            Debug.LogError("Assegna entrambe le immagini nel Inspector!");
            return;
        }

        // Salva immagine modello
        SaveReferenceImage(imageReference);

        float similarity = CompareLineDrawingsIgnoreBackground(imageReference, imagePlayer, thresholdLine, thresholdBG);
        Debug.Log($"Similarità (Jaccard Index senza sfondo): {similarity * 100f:F2}%");
    }

    void SaveReferenceImage(Texture2D texture)
    {
        byte[] pngData = texture.EncodeToPNG();
        if (pngData != null)
        {
            string path = Path.Combine(Application.persistentDataPath, "modello.png");
            File.WriteAllBytes(path, pngData);
            Debug.Log($"Immagine modello salvata in: {path}");
        }
        else
        {
            Debug.LogWarning("Impossibile convertire immagine in PNG.");
        }
    }

    float CompareLineDrawingsIgnoreBackground(Texture2D refTex, Texture2D playerTex, float lineThresh, float bgThresh)
    {
        Texture2D playerResized = ResizeTexture(playerTex, refTex.width, refTex.height);

        bool[] refBinary = TextureToBinaryArray(refTex, lineThresh);
        bool[] playerBinary = TextureToBinaryArray(playerResized, lineThresh);

        bool[] refMask = TextureToMaskArray(refTex, bgThresh);
        bool[] playerMask = TextureToMaskArray(playerResized, bgThresh);

        int intersection = 0;
        int unionCount = 0;

        for (int i = 0; i < refBinary.Length; i++)
        {
            if (refMask[i] || playerMask[i])
            {
                bool unionPixel = refBinary[i] || playerBinary[i];
                if (unionPixel)
                {
                    unionCount++;
                    if (refBinary[i] && playerBinary[i])
                        intersection++;
                }
            }
        }

        if (unionCount == 0)
            return 0f;

        return (float)intersection / unionCount;
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
            float gray = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            binary[i] = gray < threshold;
        }

        return binary;
    }

    bool[] TextureToMaskArray(Texture2D tex, float bgThreshold)
    {
        Color[] pixels = tex.GetPixels();
        bool[] mask = new bool[pixels.Length];

        for (int i = 0; i < pixels.Length; i++)
        {
            float gray = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            mask[i] = gray < bgThreshold;
        }

        return mask;
    }
}
