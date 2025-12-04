using UnityEngine;
using UnityEngine.UI;

public class PitchDetector : MonoBehaviour
{
    [Header("Impostazioni Sensibilità")]
    [Tooltip("Alza questo valore per ignorare i rumori di fondo (es. tastiera). Prova 0.02 o 0.05.")]
    [Range(0.0001f, 0.1f)]
    public float sensitivityThreshold = 0.02f; // Default alzato per evitare click tastiera

    [Header("Debug")]
    public float RmsValue; // Volume attuale
    public float PitchValue; // Frequenza in Hz
    public string CurrentNote; // Nome della nota

    private AudioSource _audioSource;
    private float[] _spectrum = new float[1024];
    private float _fSample;
    private string _microphoneID;

    private string[] _noteNames = { "Do", "Do#", "Re", "Re#", "Mi", "Fa", "Fa#", "Sol", "Sol#", "La", "La#", "Si" };

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _fSample = AudioSettings.outputSampleRate;

        if (Microphone.devices.Length > 0)
        {
            _microphoneID = Microphone.devices[0];
            _audioSource.clip = Microphone.Start(_microphoneID, true, 10, 44100);
            _audioSource.loop = true;
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
        _audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);

        // Calcolo Volume (RMS)
        float sum = 0;
        for (int i = 0; i < _spectrum.Length; i++)
        {
            sum += _spectrum[i] * _spectrum[i];
        }
        RmsValue = Mathf.Sqrt(sum / _spectrum.Length);

        // Trova il picco
        float maxV = 0;
        var maxN = 0;

        for (var i = 0; i < _spectrum.Length; i++)
        {
            // QUI USIAMO LA VARIABILE PUBBLICA 'sensitivityThreshold'
            if (_spectrum[i] <= maxV || _spectrum[i] < sensitivityThreshold) continue;

            maxV = _spectrum[i];
            maxN = i;
        }

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
            // Se il suono è sotto la soglia, resetta tutto
            PitchValue = 0;
            CurrentNote = ""; // Stringa vuota = Silenzio
        }

        if (PitchValue > 0)
        {
            CurrentNote = GetNoteFromFrequency(PitchValue);
        }
    }

    string GetNoteFromFrequency(float frequency)
    {
        var noteNum = 12 * Mathf.Log(frequency / 440f, 2) + 69;
        int roundedNote = Mathf.RoundToInt(noteNum);
        int noteIndex = roundedNote % 12;
        if (noteIndex < 0) noteIndex += 12;
        return _noteNames[noteIndex];
    }
}