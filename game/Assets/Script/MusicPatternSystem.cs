using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // NECESSARIO PER IL NUOVO INPUT SYSTEM

public class MusicPatternSystem : MonoBehaviour
{
    [Header("Riferimenti")]
    public PitchDetector detector;
    public GameObject sleepWavePrefab;
    public Transform playerTransform;

    [Header("UI")]
    public GameObject spellPanel;
    public TextMeshProUGUI requestText; // Es: "Richiesto: Do - Re - Mi"
    public TextMeshProUGUI recordingText; // Es: "Sentito: Do - Re..."
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI feedbackText;

    [Header("Impostazioni")]
    public List<string> targetSequence = new List<string> { "Do", "Re", "Mi", "Fa" };
    public float recordingTime = 10f;
    [Range(0f, 1f)] public float toleranceThreshold = 0.6f; // 0.6 = Basta il 60% di somiglianza

    private bool isRecording = false;
    private List<string> recordedNotes = new List<string>();

    void Start()
    {
        if (spellPanel) spellPanel.SetActive(false);
    }

    void Update()
    {
        // --- MODIFICA PER IL NUOVO INPUT SYSTEM ---
        // Controlliamo se la tastiera esiste e se il tasto SPAZIO è stato premuto in questo frame
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isRecording)
        {
            StartCoroutine(RecordPatternRoutine());
        }
    }

    IEnumerator RecordPatternRoutine()
    {
        isRecording = true;
        recordedNotes.Clear();

        // PAUSA GIOCO
        Time.timeScale = 0f;
        AudioListener.pause = false; // Microfono attivo

        if (spellPanel) spellPanel.SetActive(true);
        if (feedbackText) feedbackText.text = "SUONA ORA!";
        UpdateUIStrings();

        float timeLeft = recordingTime;
        string lastAddedNote = "";

        // --- FASE DI REGISTRAZIONE ---
        while (timeLeft > 0)
        {
            timeLeft -= Time.unscaledDeltaTime;
            if (timerText) timerText.text = timeLeft.ToString("F1") + "s";

            string currentNote = detector.CurrentNote;

            // Logica di Registrazione Intelligente:
            // Aggiungiamo la nota solo se è diversa dall'ultima registrata (Deduplicazione)
            if (!string.IsNullOrEmpty(currentNote) && currentNote != lastAddedNote)
            {
                recordedNotes.Add(currentNote);
                lastAddedNote = currentNote;
                UpdateUIStrings(); // Aggiorna la lista a video
            }

            yield return null;
        }

        // --- FASE DI ANALISI ---
        float similarity = CalculateSimilarity(recordedNotes, targetSequence);
        Debug.Log($"Somiglianza: {similarity * 100}%");

        if (similarity >= toleranceThreshold)
        {
            if (feedbackText) feedbackText.text = "<color=green>INCANTESIMO RIUSCITO!</color>";
            SpawnWave();
        }
        else
        {
            if (feedbackText) feedbackText.text = $"<color=red>FALLITO ({similarity * 100:F0}%)</color>";
        }

        yield return new WaitForSecondsRealtime(2f); // Leggi risultato

        // RESET
        if (spellPanel) spellPanel.SetActive(false);
        Time.timeScale = 1f;
        isRecording = false;
    }

    // Algoritmo di Levenshtein (Calcola la distanza tra due liste)
    float CalculateSimilarity(List<string> recorded, List<string> target)
    {
        if (recorded.Count == 0) return 0f;

        int n = recorded.Count;
        int m = target.Count;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) d[i, 0] = i;
        for (int j = 0; j <= m; j++) d[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (target[j - 1] == recorded[i - 1]) ? 0 : 1;
                d[i, j] = Mathf.Min(
                    Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        int errors = d[n, m];
        int maxLength = Mathf.Max(n, m);

        return 1f - ((float)errors / maxLength);
    }

    void SpawnWave()
    {
        if (sleepWavePrefab && playerTransform)
        {
            Instantiate(sleepWavePrefab, playerTransform.position, Quaternion.identity);
        }
    }

    void UpdateUIStrings()
    {
        if (requestText) requestText.text = "Target: " + string.Join(" - ", targetSequence);

        int start = Mathf.Max(0, recordedNotes.Count - 6);
        List<string> displayList = recordedNotes.GetRange(start, recordedNotes.Count - start);
        if (recordingText) recordingText.text = "Tu: " + string.Join(" - ", displayList);
    }
}