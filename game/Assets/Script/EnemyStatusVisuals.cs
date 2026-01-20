using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyStatusVisuals : MonoBehaviour
{
    [Header("UI References")]
    public Canvas worldCanvas;
    public Image iconImage;
    public TextMeshProUGUI textLabel;

    [Header("Icons")]
    public Sprite alertSprite;   // Il "!" (Vista)
    public Sprite hearingSprite; // /// NUOVO: L'Orecchio o "?" (Udito)
    public Sprite searchSprite;
    public Sprite coffeeSprite;

    private Camera mainCamera;
    private Coroutine currentRoutine;

    void Start()
    {
        mainCamera = Camera.main;
        HideAll();
    }

    void LateUpdate()
    {
        if (worldCanvas != null && mainCamera != null)
        {
            worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - mainCamera.transform.position);
        }
    }

    // /// NUOVO METODO: Gestisce l'allerta specifica (Vista vs Udito)
    public void TriggerDetection(bool isAudio)
    {
        HideAll();
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        if (iconImage != null)
        {
            // Scegli l'immagine in base al tipo di rilevamento
            if (isAudio && hearingSprite != null)
            {
                iconImage.sprite = hearingSprite;
                iconImage.color = new Color(1f, 0.5f, 0f); // Arancione per l'udito? O giallo
            }
            else if (alertSprite != null)
            {
                iconImage.sprite = alertSprite;
                iconImage.color = Color.red; // Rosso per la vista diretta
            }

            iconImage.enabled = true;

            // Effetti
            transform.localScale = Vector3.zero;
            StartCoroutine(PopEffect());
            currentRoutine = StartCoroutine(HideAfterDelay(2f));
        }
    }

    public void UpdateStatus(STATE state)
    {
        // Se stiamo inseguendo, NON resettiamo tutto qui, 
        // perché lo gestisce TriggerDetection chiamato da NemicoScript.
        if (state == STATE.CHASING) return;

        HideAll();
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        switch (state)
        {
            // CASE CHASING RIMOSSO DA QUI (gestito separatamente)

            case STATE.SEARCHING:
                if (iconImage != null && searchSprite != null)
                {
                    iconImage.sprite = searchSprite;
                    iconImage.enabled = true;
                    iconImage.color = Color.yellow;
                    transform.localScale = Vector3.one;
                }
                break;

            case STATE.SLEEPING:
                if (textLabel != null)
                {
                    textLabel.text = "Zzz...";
                    textLabel.enabled = true;
                    textLabel.color = Color.white;
                    transform.localScale = Vector3.one;
                    currentRoutine = StartCoroutine(PulseEffect(textLabel.transform));
                }
                break;

            case STATE.DISTRACTED:
                if (iconImage != null && coffeeSprite != null)
                {
                    iconImage.sprite = coffeeSprite;
                    iconImage.enabled = true;
                    iconImage.color = Color.white;
                    transform.localScale = Vector3.one;
                    currentRoutine = StartCoroutine(HideAfterDelay(3f));
                }
                break;

            case STATE.VIGILE:
                HideAll();
                break;
        }
    }

    void HideAll()
    {
        if (iconImage != null) iconImage.enabled = false;
        if (textLabel != null) textLabel.enabled = false;
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideAll();
    }

    IEnumerator PopEffect()
    {
        float timer = 0;
        while (timer < 0.2f)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0, 1.2f, timer / 0.2f);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    IEnumerator PulseEffect(Transform target)
    {
        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.2f;
            target.localScale = Vector3.one * scale;
            yield return null;
        }
    }
}