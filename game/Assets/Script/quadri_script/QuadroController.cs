using UnityEngine;
using UnityEngine.UI; // Se usi la UI per mostrare le info
using UnityEngine.InputSystem;


public class QuadroController : MonoBehaviour
{
    public DatiQuadro dati; // Trascina qui il file ScriptableObject creato
    
    [Space]
    public MeshRenderer telaRenderer; // Il piano/mesh dove mostrare l'immagine

    public PannelloQuadro pannello;
    private bool inTrigger = false;

    private Texture2D texturePrincipaleCache;

    private bool isSee = false;





    void Start()
    {
        if (dati != null)
            ConfiguraQuadro();
        pannello.attiva(false);
    }


    void Update()
    {
        if (PauseMenu.GameIsPaused) { Debug.Log("PauseMenu gameISpaused true"); return; }
        if (inTrigger && Mouse.current.leftButton.wasPressedThisFrame && GameManager.Instance.CurrentState != GameState.Thief)
        {
            Debug.Log(dati.titolo);
            pannello.attiva(true);
            pannello.MostraQuadro(dati.immaginePrincipale, dati.info);
            if (!isSee)
            {
                isSee = true;
            
                if(MaestroManager.Instance != null)
                    MaestroManager.Instance.SegnalaQuadroVisto(dati.titolo);
                
            }
        }
    }

    public void ConfiguraQuadro()
    {
        if (dati.immaginePrincipale != null)
        {
            // Crea e cache la texture principale
            texturePrincipaleCache = SpriteToTexture2D(dati.immaginePrincipale);
            texturePrincipaleCache.filterMode = FilterMode.Bilinear;
            texturePrincipaleCache.wrapMode = TextureWrapMode.Clamp;
            
            // Applica la texture al materiale
            telaRenderer.material.mainTexture = texturePrincipaleCache;
            
            Debug.Log($"Quadro caricato: {dati.titolo}");
        }
        else
        {
            Debug.LogWarning($"Immagine principale mancante per: {dati.titolo}");
        }
    }
    
    // Funzione per convertire Sprite in Texture2D
    private Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            // Lo sprite è parte di un atlas, dobbiamo estrarre la porzione corretta
            Texture2D newTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels(
                (int)sprite.rect.x,
                (int)sprite.rect.y,
                (int)sprite.rect.width,
                (int)sprite.rect.height
            );
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
        else
        {
            // Lo sprite usa l'intera texture
            return sprite.texture;
        }
    }

    // Esempio di funzione per scambiare le immagini (confronto)
    public void MostraConfronto(bool attiva)
    {
        // telaRenderer.material.mainTexture = attiva ? dati.immagineConfronto : dati.immaginePrincipale;
    }


        private void OnTriggerEnter(Collider other)
    {
        // Il player entra nel trigger
        if (other.CompareTag("Player"))
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Il player esce dal trigger
        if (other.CompareTag("Player"))
        {
            inTrigger = false;
        }
    }
}