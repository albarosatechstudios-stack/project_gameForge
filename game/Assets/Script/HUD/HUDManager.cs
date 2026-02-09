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
    private SmokeFromBlow smokeIntensity;
    private SelfDestroy smokeLifeTime;
    private SpawnFumogeno spawnFumogeno;

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
            scriptCaffettiera = FindAnyObjectByType<SpawnCaffettiera>();

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
                testoCaffettiera.text = "CAFFE' PRONTO [E]";
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
        if (smokeIntensity == null && smokeLifeTime == null)
        {
            smokeIntensity = FindAnyObjectByType<SmokeFromBlow>();
            smokeLifeTime = FindAnyObjectByType<SelfDestroy>();
            spawnFumogeno = FindAnyObjectByType<SpawnFumogeno>();

            // Se ancora non lo trova, nascondi il testo e esci
            if (smokeLifeTime == null)
            {
                if (testoFumo != null && GameManager.Instance.CurrentState == GameState.Thief)
                {
                    testoFumo.text = "FUMO: PRONTO  [Q]";
                    testoFumo.color = Color.green; 
                    return;
                }
            }
        }

        // 2. Se abbiamo il riferimento, aggiorniamo la UI
        if (testoFumo != null && GameManager.Instance.CurrentState == GameState.Thief)
        {
            
            // Calcoli
            float percentuale = (smokeIntensity.currentSmokeLevel / smokeIntensity.maxFill) * 100f;


           // Visualizzazione (Mostra solo se c'� un minimo di fumo, es > 1%)
            if (spawnFumogeno.IsReady())
            {
                testoFumo.text = "FUMO: PRONTO  [Q]";
                testoFumo.color = Color.green; // Verde come il caff� pronto
            }
            else
            {
                testoFumo.text = $"INTENSITA' FUMO: {percentuale:F0}%\n(RICARICA: {smokeLifeTime.GetTimeRemaining():F1}s)";
                testoFumo.color = Color.red;
            }
        }
    }
}