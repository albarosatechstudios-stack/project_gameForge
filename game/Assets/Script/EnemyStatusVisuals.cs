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
    public Sprite alertSprite;
    public Sprite searchSprite;
    public Sprite coffeeSprite;

    private Camera mainCamera;
    private Coroutine currentRoutine; // Per gestire i timer

    void Start()
    {
        mainCamera = Camera.main;
        HideAll();
    }

    void LateUpdate()
    {
        // Il canvas guarda sempre la telecamera
        if (worldCanvas != null && mainCamera != null)
        {
            worldCanvas.transform.rotation = Quaternion.LookRotation(worldCanvas.transform.position - mainCamera.transform.position);
        }
    }

    public void UpdateStatus(STATE state)
    {
        // 1. Resetta tutto prima di mostrare il nuovo stato
        HideAll();
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        switch (state)
        {
            case STATE.CHASING:
                // IL "!" deve essere uno shock: appare e poi sparisce dopo 2 secondi
                if (iconImage != null && alertSprite != null)
                {
                    iconImage.sprite = alertSprite;
                    iconImage.enabled = true;
                    iconImage.color = Color.red;
                    // Animazione Pop
                    transform.localScale = Vector3.zero;
                    StartCoroutine(PopEffect());
                    // Nascondi dopo 2 secondi
                    currentRoutine = StartCoroutine(HideAfterDelay(2f));
                }
                break;

            case STATE.SEARCHING:
                // Il "?" può rimanere fisso finché cerca, o sparire. Facciamolo fisso.
                if (iconImage != null && searchSprite != null)
                {
                    iconImage.sprite = searchSprite;
                    iconImage.enabled = true;
                    iconImage.color = Color.yellow;
                    transform.localScale = Vector3.one;
                }
                break;

            case STATE.SLEEPING:
                // "Zzz" deve rimanere SEMPRE finché dorme, e magari pulsare
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
                // L'icona oggetto appare per 3 secondi poi sparisce
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
                // Nascondi tutto
                HideAll();
                break;
        }
    }

    void HideAll()
    {
        if (iconImage != null) iconImage.enabled = false;
        if (textLabel != null) textLabel.enabled = false;
    }

    // --- COROUTINES PER EFFETTI E TEMPO ---

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideAll();
    }

    IEnumerator PopEffect()
    {
        // Semplice effetto di ingrandimento veloce
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
        // Effetto respiro lento per "Zzz"
        while (true)
        {
            float scale = 1f + Mathf.Sin(Time.time * 2f) * 0.2f; // Oscilla tra 0.8 e 1.2
            target.localScale = Vector3.one * scale;
            yield return null;
        }
    }
}