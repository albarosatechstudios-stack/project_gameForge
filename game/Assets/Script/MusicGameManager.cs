using System.Collections.Generic;
using UnityEngine;
using TMPro; // Assicurati di usare TextMeshPro per la UI

public class MusicGameManager : MonoBehaviour
{
    public PitchDetector detector; // Trascina qui lo script precedente
    public TextMeshProUGUI noteDisplayUI; // Testo che mostra cosa suonare
    public TextMeshProUGUI feedbackUI;    // Testo "Corretto!" o "Sbagliato"

    // Lo spartito (Lista di note da suonare in sequenza)
    public List<string> sheetMusic = new List<string> { "Do", "Re", "Mi", "Fa", "Sol" };

    private int _currentIndex = 0;
    private float _matchTimer = 0f;
    private float _requiredDuration = 0.5f; // Quanto a lungo deve tenere la nota (secondi)

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (_currentIndex >= sheetMusic.Count)
        {
            feedbackUI.text = "BRANO COMPLETATO!";
            feedbackUI.color = Color.green;
            return;
        }

        string targetNote = sheetMusic[_currentIndex];
        string detectedNote = detector.CurrentNote;

        // Controlla se la nota suonata corrisponde a quella richiesta
        if (detectedNote == targetNote)
        {
            _matchTimer += Time.deltaTime;
            feedbackUI.text = "Mantieni...";
            feedbackUI.color = Color.yellow;

            // Se la nota è tenuta abbastanza a lungo
            if (_matchTimer >= _requiredDuration)
            {
                NoteCompleted();
            }
        }
        else
        {
            _matchTimer = 0; // Reset se sbaglia o smette di suonare
            feedbackUI.text = "Suona: " + targetNote;
            feedbackUI.color = Color.white;
        }
    }

    void NoteCompleted()
    {
        Debug.Log("Nota presa: " + sheetMusic[_currentIndex]);
        _currentIndex++;
        _matchTimer = 0;

        // EVENTO: Qui puoi far partire particellari, suoni, o animazioni
        UpdateUI();
    }

    void UpdateUI()
    {
        if (_currentIndex < sheetMusic.Count)
        {
            noteDisplayUI.text = "Nota Corrente: " + sheetMusic[_currentIndex];
        }
    }
}