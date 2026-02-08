using UnityEngine;
using TMPro; // Assicurati di usare TextMeshPro

public class QuestHUD : MonoBehaviour
{
    [Header("Riferimenti UI")]
    public TextMeshProUGUI textQuest;

    private void OnEnable()
    {
        // Ci iscriviamo all'evento del MaestroManager
        MaestroManager.OnQuestUpdated += UpdateHud;
    }

    private void OnDisable()
    {
        // Ci disiscriviamo per evitare errori di memoria
        MaestroManager.OnQuestUpdated -= UpdateHud;
    }

    private void Start()
    {
        // Aggiornamento iniziale all'avvio del gioco
        UpdateHud();
    }

    public void UpdateHud()
    {
        if (MaestroManager.Instance == null) return;

        QuestMaestro fase = MaestroManager.Instance.faseAttuale;
        string text = "";

        // Qui decidi cosa scrivere per ogni fase
        switch (fase)
        {
            case QuestMaestro.Inizio:
                text = "Trova il Maestro e parlagli.";
                break;
            case QuestMaestro.DeveVedereQuadro:
                text = "Trova il quadro: " + MaestroManager.Instance.getNameObective()  +" nel museo.";
                break;
            case QuestMaestro.QuadroVisto:
                text = "Hai visto il quadro. Torna a riferire al Maestro.";
                break;
            case QuestMaestro.CreazioneFalso:
                text = "Lavora sulla tela per creare il falso.";
                break;
            case QuestMaestro.FalsoPronto:
                text = "Il falso è pronto. Scambialo o parla col Maestro.";
                break;
            case QuestMaestro.FineGioco:
                text = "Incrociamo le dita.";
                break;
        }

        textQuest.text = text;
    }
}