using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HUDManagerTMP : MonoBehaviour
{
    [Header("Riferimenti UI (Assegna questi nell'Inspector)")]
    public TextMeshProUGUI testoCaffettiera;
    public TextMeshProUGUI testoFumo;

    // Variabili private per memorizzare i riferimenti trovati
    private SpawnCaffettiera scriptCaffettiera;
    private SmokeFromBlow scriptFumo;

    void Update()
    {
        if (SceneManager.GetActiveScene().name == "LivelloAlpha")
        {
            GestisciUI_Caffettiera();
            GestisciUI_Fumo();
        }
    }

    void GestisciUI_Caffettiera()
    {
        // 1. Se non abbiamo il riferimento, proviamo a cercarlo
        if (scriptCaffettiera == null)
        {
            scriptCaffettiera = FindObjectOfType<SpawnCaffettiera>();

            // Se ancora non lo trova, nascondi il testo e esci
            if (scriptCaffettiera == null)
            {
                if (testoCaffettiera != null) testoCaffettiera.text = "";
                return;
            }
        }

        // 2. Se abbiamo il riferimento, aggiorniamo la UI
        if (testoCaffettiera != null && GameManager.Instance.CurrentState == GameState.Thief)
        {
            if (scriptCaffettiera.IsReady())
            {
                testoCaffettiera.text = "CAFFÈ PRONTO [E]";
                testoCaffettiera.color = Color.green;
            }
            else
            {
                float tempo = scriptCaffettiera.GetTimeRemaining();
                testoCaffettiera.text = $"Ricarica: {tempo:F1}s";
                testoCaffettiera.color = Color.red;
            }
        }
    }

    void GestisciUI_Fumo()
    {
        // 1. Se non abbiamo il riferimento, proviamo a cercarlo
        if (scriptFumo == null)
        {
            scriptFumo = FindObjectOfType<SmokeFromBlow>();

            // Se ancora non lo trova, nascondi il testo e esci
            if (scriptFumo == null)
            {
                if (testoFumo != null && GameManager.Instance.CurrentState == GameState.Thief)
                {
                    testoFumo.text = "FUMO: PRONTO  [Q]";
                    testoFumo.color = Color.green; // Verde come il caffè pronto};
                    return;
                }
            }
        }

        // 2. Se abbiamo il riferimento, aggiorniamo la UI
        if (testoFumo != null && GameManager.Instance.CurrentState == GameState.Thief)
        {
            
            // Calcoli
            float percentuale = (scriptFumo.currentSmokeLevel / scriptFumo.maxFill) * 100f;

            float tempoVita = 0f;
            if (scriptFumo.decayRate > 0)
                tempoVita = scriptFumo.currentSmokeLevel / scriptFumo.decayRate;

            // Visualizzazione (Mostra solo se c'è un minimo di fumo, es > 1%)
            if (percentuale > 1f)
            {
                testoFumo.text = $"FUMO: {percentuale:F0}% ({tempoVita:F1}s)";
                testoFumo.color = Color.white;
            }
            else
            {
                testoFumo.text = "FUMO: PRONTO  [Q]";
                testoFumo.color = Color.green; // Verde come il caffè pronto
            }
        }
    }
}