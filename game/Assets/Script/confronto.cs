using UnityEngine;
using System.IO;

public class SimpleLineComparerIgnoreBG : MonoBehaviour
{
    [Header("Impostazioni Confronto")]
    [Tooltip("Quanto deve essere scuro un pixel per essere considerato linea (0=nero, 1=bianco)")]
    [Range(0, 1)] public float thresholdLine = 0.5f;

    [Tooltip("Soglia per ignorare lo sfondo carta (solitamente alto, es. 0.9)")]
    [Range(0, 1)] public float thresholdBG = 0.9f;

    [Tooltip("Raggio di tolleranza in pixel. Aumentalo per essere più buono con il giocatore.")]
    [Range(0, 10)] public int toleranceRadius = 4;
    [HideInInspector] public Texture2D textureRisultato;

    /// <summary>
    /// Funzione principale da chiamare per avviare il confronto.
    /// </summary>
    public float CompareWithSavedDrawing(Texture2D imageReference)
    {
        // 1. Controlli preliminari
        if (imageReference == null)
        {
            Debug.LogError("ERRORE: Manca l'immagine di riferimento (Line Art) nello script!");
            return -1f;
        }

        string savedPath = Path.Combine(Application.persistentDataPath, "Disegni", "saved_drawing.png");
        if (!File.Exists(savedPath))
        {
            Debug.LogError($"ERRORE: Non trovo il disegno del player in: {savedPath}");
            return -1f;
        }

        // 2. Caricamento disegno Player
        byte[] fileData = File.ReadAllBytes(savedPath);
        Texture2D imagePlayer = new Texture2D(2, 2);
        if (!imagePlayer.LoadImage(fileData))
        {
            Debug.LogError("ERRORE: Impossibile caricare il file del player come Texture.");
            return -1f;
        }

        // 3. Esecuzione algoritmo
        float similarity = CalculateSimilarityWithTolerance(imageReference, imagePlayer);

        Debug.Log($"RISULTATO: Similarità calcolata: {similarity * 100f:F2}%");
        return similarity * 100f;
    }

    /// <summary>
    /// Logica centrale di confronto con Tolleranza (Dilatazione).
    /// </summary>
    private float CalculateSimilarityWithTolerance(Texture2D refTex, Texture2D playerTex)
    {
        int w = refTex.width;
        int h = refTex.height;

        // A. Ridimensioniamo il disegno del player per combaciare col modello
        Texture2D playerResized = ResizeTexture(playerTex, w, h);

        // B. Convertiamo le immagini in mappe di true/false (true = c'è inchiostro)
        bool[] refBinary = TextureToBinaryArray(refTex, thresholdLine);
        bool[] playerBinary = TextureToBinaryArray(playerResized, thresholdLine);

        // C. Creiamo una versione "Ingrassata" del riferimento per la tolleranza
        //    Questo permette al player di essere leggermente impreciso.
        bool[] refDilated = DilateBinary(refBinary, w, h, toleranceRadius);

        // D. Generiamo l'immagine di Debug per capire cosa è successo
        SaveDebugImage(refBinary, playerBinary, refDilated, w, h);

        // E. Calcolo Punteggio (Jaccard Index modificato)
        int matches = 0;    // Pixel giusti
        int totalInk = 0;   // Totale pixel considerati (unione)

        for (int i = 0; i < refBinary.Length; i++)
        {
            bool isPlayerInk = playerBinary[i];
            bool isRefInk = refDilated[i]; // Usiamo quello dilatato per il controllo
            bool isRefOriginal = refBinary[i];

            // Consideriamo il pixel solo se c'è inchiostro di qualcuno (evitiamo lo sfondo vuoto infinito)
            if (isPlayerInk || isRefOriginal)
            {
                totalInk++; // È un punto di interesse

                if (isPlayerInk && isRefInk)
                {
                    matches++; // Il player ha disegnato dentro la zona di tolleranza
                }
            }
        }

        if (totalInk == 0) return 0f;

        return (float)matches / totalInk;
    }

    /// <summary>
    /// Espande i pixel 'true' creando un'area più larga (Buffer/Tolleranza).
    /// </summary>
    private bool[] DilateBinary(bool[] input, int width, int height, int radius)
    {
        if (radius <= 0) return input;

        bool[] output = new bool[input.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                if (input[i]) // Se qui c'è una linea originale
                {
                    // Accendi tutti i pixel vicini nel raggio
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int ny = y + dy;
                            int nx = x + dx;
                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                output[ny * width + nx] = true;
                            }
                        }
                    }
                }
            }
        }
        return output;
    }

    /// <summary>
    /// Crea l'immagine debug: Verde=Ok, Blu=Mancato, Rosso=Errore
    /// </summary>
    private void SaveDebugImage(bool[] refOriginal, bool[] playerBin, bool[] refDilated, int w, int h)
    {
        Texture2D debugTex = new Texture2D(w, h);
        Color[] pixels = new Color[w * h];

        for (int i = 0; i < pixels.Length; i++)
        {
            bool p = playerBin[i];       // Disegno Player
            bool rOrig = refOriginal[i]; // Linea Sottile Modello
            bool rWide = refDilated[i];  // Area Tolleranza

            if (p && rWide)
            {
                pixels[i] = Color.green; // MATCH (Player ha beccato la zona)
            }
            else if (rOrig && !p)
            {
                pixels[i] = Color.blue;  // MISSING (C'era una linea ma player non l'ha fatta)
            }
            else if (p && !rWide)
            {
                pixels[i] = Color.red;   // WRONG (Player ha scarabocchiato fuori)
            }
            else
            {
                pixels[i] = Color.white; // SFONDO
            }
        }

        debugTex.SetPixels(pixels);
        debugTex.Apply();
        textureRisultato = debugTex;

        byte[] pngData = debugTex.EncodeToPNG();
        string path = Path.Combine(Application.persistentDataPath, "confronto_debug.png");
        File.WriteAllBytes(path, pngData);
        Debug.Log($"Immagine debug salvata in: {path}");
    }

    // --- FUNZIONI DI SUPPORTO ---

    private Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
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

    private bool[] TextureToBinaryArray(Texture2D tex, float threshold)
    {
        Color[] pixels = tex.GetPixels();
        bool[] binary = new bool[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            // Formula luminosità standard
            float gray = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            binary[i] = gray < threshold; // Se è scuro, è inchiostro (true)
        }
        return binary;
    }
}