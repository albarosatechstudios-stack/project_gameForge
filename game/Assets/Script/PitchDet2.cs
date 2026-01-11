using UnityEngine;
using UnityEngine.UI;

public class PitchDet2 : MonoBehaviour
{
    [Header("Impostazioni Sensibilità")]
    [Tooltip("Soglia volume minimo. Alza se sente rumori di fondo (es. 0.02).")]
    [Range(0.0001f, 0.1f)]
    public float sensitivityThreshold = 0.02f;

    [Header("Impostazioni Stabilità")]
    [Tooltip("Quanti frame la nota deve rimanere uguale prima di essere confermata. Aumenta per meno sfarfallio, diminuisci per più reattività.")]
    public int stableFramesRequired = 5;

    [Header("Debug")]
    public float RmsValue; // Volume attuale
    public float PitchValue; // Frequenza in Hz
    public string CurrentNote; // La nota CONFERMATA e stabile
    public string RawNote;     // La nota istantanea (ballerina)

    private AudioSource _audioSource;
    // AUMENTATO A 4096 per maggiore precisione sulle frequenze basse
    private float[] _spectrum = new float[4096];
    private float _fSample;
    private string _microphoneID;

    // Variabili per la stabilizzazione
    private string _potentialNote = "";
    private int _stabilityCounter = 0;

    private string[] _noteNames = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" };

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _fSample = AudioSettings.outputSampleRate;

        if (Microphone.devices.Length > 0)
        {
            _microphoneID = Microphone.devices[0];
            // Avviamo il microfono
            _audioSource.clip = Microphone.Start(_microphoneID, true, 10, 44100);
            _audioSource.loop = true;
            // Loop di attesa finché non registra davvero
            while (!(Microphone.GetPosition(_microphoneID) > 0)) { }
            _audioSource.Play();
        }
        else
        {
            Debug.LogError("Nessun microfono trovato!");
        }
    }

    void Update()
    {
        AnalyzeSound();
    }

    void AnalyzeSound()
    {
        // Ottieni spettro ad alta risoluzione
        _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);

        // 1. Calcolo Volume (RMS)
        float sum = 0;
        for (int i = 0; i < _spectrum.Length; i++)
        {
            sum += _spectrum[i] * _spectrum[i];
        }
        RmsValue = Mathf.Sqrt(sum / _spectrum.Length);

        // 2. Trova il picco di frequenza
        float maxV = 0;
        var maxN = 0;

        for (var i = 0; i < _spectrum.Length; i++)
        {
            // Filtro Rumore e Frequenze Inutili
            // Ignoriamo frequenze sotto i 60Hz (rumble) e sopra i 1500Hz (troppo acute per voce base) per evitare errori
            // Nota: 44100Hz / 4096 campioni = ~10.7 Hz per bin.
            // Indice 5 ~= 53Hz. Indice 150 ~= 1600Hz.
            if (i < 5 || i > 200) continue;

            if (_spectrum[i] <= maxV || _spectrum[i] < sensitivityThreshold) continue;

            maxV = _spectrum[i];
            maxN = i;
        }

        // 3. Calcolo Pitch preciso
        if (maxN > 0 && maxN < _spectrum.Length - 1)
        {
            float freqN = maxN;
            var dL = _spectrum[maxN - 1] / _spectrum[maxN];
            var dR = _spectrum[maxN + 1] / _spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);

            PitchValue = freqN * (_fSample / 2) / _spectrum.Length;
        }
        else
        {
            PitchValue = 0;
        }

        // 4. Conversione in Nota con STABILIZZAZIONE
        if (PitchValue > 0)
        {
            string detectedNote = GetNoteFromFrequency(PitchValue);
            RawNote = detectedNote; // Per debug, vediamo cosa sente all'istante

            // Logica BUFFER:
            // Se la nota rilevata è uguale a quella "potenziale" del frame precedente...
            if (detectedNote == _potentialNote)
            {
                _stabilityCounter++;
                // Se è stabile per tot frame, confermala come Nota Corrente
                if (_stabilityCounter >= stableFramesRequired)
                {
                    CurrentNote = detectedNote;
                }
            }
            else
            {
                // La nota è cambiata, resettiamo il contatore
                _potentialNote = detectedNote;
                _stabilityCounter = 0;
            }
        }
        else
        {
            // Silenzio
            RawNote = "";
            _stabilityCounter = 0;
            // Opzionale: se c'è silenzio prolungato resetta CurrentNote, 
            // ma spesso è meglio tenerla "in memoria" per non far fallire il gioco se prendi fiato.
            // CurrentNote = ""; 
        }
    }

    string GetNoteFromFrequency(float frequency)
    {
        // Formula standard
        var noteNum = 12 * Mathf.Log(frequency / 440f, 2) + 69;
        int roundedNote = Mathf.RoundToInt(noteNum);

        int noteIndex = roundedNote % 12;
        if (noteIndex < 0) noteIndex += 12;

        return _noteNames[noteIndex];
    }
}