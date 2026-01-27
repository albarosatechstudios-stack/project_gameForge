using UnityEngine;
using UnityEngine.UI;

public class MaestroVisuals : MonoBehaviour
{
    [Header("Riferimenti")]
    public Canvas worldCanvas;
    public Image iconImage;

    [Header("Icone")]
    public Sprite visitorSprite; // "..." o Smile
    public Sprite thiefSprite;   // "!"
    public Sprite questSprite;   // "?" (Per il quadro)

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (MaestroManager.Instance == null) return;

        // 1. Controllo se devo mostrare l'icona (Leggo dal Manager)
        bool deveMostrare = MaestroManager.Instance.iconaDaMostrare;

        if (deveMostrare)
        {
            // Se devo mostrarla, calcolo QUALE mostrare e la attivo
            AggiornaGrafica();
        }
        else
        {
            // Se non devo mostrarla, spengo tutto
            if (iconImage.enabled) iconImage.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (worldCanvas != null && mainCamera != null)
        {
            worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - mainCamera.transform.position);
        }
    }

    void AggiornaGrafica()
    {
        GameState stato = MaestroManager.Instance.statoMentaleMaestro;
        Sprite spriteFinale = null;

        // PRIORITÀ GRAFICA
        // 1. Se è Ladro -> Esclamativo
        if (stato == GameState.Thief)
        {
            spriteFinale = thiefSprite;
        }
        // 2. Se ho visto il quadro (e non sono ladro) -> Interrogativo
        else if (MaestroManager.Instance.segretoQuadroSbloccato)
        {
            // Nota: qui potresti voler mostrare il "?" solo se non ne abbiamo ancora parlato.
            // Ma per semplicità, se l'icona è accesa e il quadro è sbloccato, mostriamo "?"
            spriteFinale = questSprite;
        }
        // 3. Default -> Nuvoletta
        else
        {
            spriteFinale = visitorSprite;
        }

        // Applico lo sprite
        if (iconImage.sprite != spriteFinale || iconImage.enabled == false)
        {
            iconImage.sprite = spriteFinale;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }
    }
}