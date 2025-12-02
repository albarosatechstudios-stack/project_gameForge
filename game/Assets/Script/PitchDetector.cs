using UnityEngine;
using UnityEngine.UI;

public class PitchDetector : MonoBehaviour
{
    public float RmsValue; // Volume
    public float PitchValue; // Frequenza in Hz
    public string CurrentNote; // Nome della nota (es. C4)

    private AudioSource _audioSource;
    private float[] _spectrum = new float[1024];
    private float _fSample;
    private string _microphoneID;

    // Note musicali (Notazione inglese)
    private string[] _noteNames = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" };

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _fSample = AudioSettings.outputSampleRate;

        // Avvia il microfono
        if (Microphone.devices.Length > 0)
        {
            _microphoneID = Microphone.devices[0];
            _audioSource.clip = Microphone.Start(_microphoneID, true, 10, 44100);
            _audioSource.loop = true;

            // Trucco per ridurre la latenza: aspetta che inizi a registrare e poi suona
            while (!(Microphone.GetPosition(_microphoneID) > 0)) { }
            _audioSource.Play();
        }
        else
        {
            Debug.LogError("Nessun microfono trovato!");
        }
        print(_microphoneID);
    }

    void Update()
    {
        AnalyzeSound();
    }

    void AnalyzeSound()
    {
        // 1. Ottieni lo spettro audio
        _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);

        // 2. Calcoliamo il Volume (RMS) - PRIMA MANCAVA QUESTO PEZZO!
        float sum = 0;
        for (int i = 0; i < _spectrum.Length; i++)
        {
            sum += _spectrum[i] * _spectrum[i]; // Somma i quadrati
        }
        RmsValue = Mathf.Sqrt(sum / _spectrum.Length); // Calcola la media quadratica

        // 3. Trova la frequenza (Pitch)
        float maxV = 0;
        var maxN = 0;

        // IMPORTANTE: Ho abbassato la soglia da 0.02f a 0.0001f per il test
        // Se il microfono è basso, 0.02 era troppo alto e ignorava tutto.
        float threshold = 0.00001f;

        for (var i = 0; i < _spectrum.Length; i++)
        {
            // Se il picco è troppo basso (rumore di fondo), ignoralo
            if (_spectrum[i] <= maxV || _spectrum[i] < threshold) continue;

            maxV = _spectrum[i];
            maxN = i;
        }

        // Se abbiamo trovato un picco valido
        if (maxN > 0 && maxN < _spectrum.Length - 1)
        {
            float freqN = maxN;
            // Interpolazione
            var dL = _spectrum[maxN - 1] / _spectrum[maxN];
            var dR = _spectrum[maxN + 1] / _spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);

            PitchValue = freqN * (_fSample / 2) / _spectrum.Length;
        }
        else
        {
            // Se non c'è abbastanza volume, resetta il pitch
            PitchValue = 0;
        }

        // 4. Converti Hz in Nota solo se c'è suono
        if (PitchValue > 0)
        {
            CurrentNote = GetNoteFromFrequency(PitchValue);
        }
    }

    string GetNoteFromFrequency(float frequency)
    {
        // Formula: NoteNum = 12 * log2(freq / 440) + 69
        var noteNum = 12 * Mathf.Log(frequency / 440f, 2) + 69;
        int roundedNote = Mathf.RoundToInt(noteNum);

        // Calcola ottava e indice nota
        int noteIndex = roundedNote % 12;
        if (noteIndex < 0) noteIndex += 12; // Gestione note basse

        // Esempio output: C, D#, A
        return _noteNames[noteIndex];
    }
}