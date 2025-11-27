using UnityEngine;

public class SoffioScript : MonoBehaviour
{

    public float sensitivity = 40f;   // quanto amplificare la voce
    public float minimum = 0.01f;     // soglia molto bassa per riconoscere anche la voce

    private AudioClip micClip;
    private string deviceName;
    private float[] samples = new float[256];

    void Start()
    {
        // Avvia il microfono
        if (Microphone.devices.Length > 0)
        {
            // controllo che microfoni ci sono
            // for (int i=0; i<Microphone.devices.Length; i++)
            // {
            //      print("device:"+ Microphone.devices[i]);  
            // }

            //NOTA su MIRO - le liste dei devices microfono non sono standard applicare soluzione riportata su MIRO
            deviceName = Microphone.devices[Microphone.devices.Length-1];
            micClip = Microphone.Start(deviceName, true, 1, 44100);
            Debug.Log("Microfono avviato: " + deviceName);
        }
        else
        {
            Debug.LogError("Nessun microfono rilevato.");
        }
    }

    void Update()
    {
        float power = GetSoundStrength();
        Debug.Log("Intensità voce/soffio: " + power);
    }

    // ----- FUNZIONE COMPLETA ---
    public float GetSoundStrength()
    {
        if (micClip == null)
            return 0f;

        int micPos = Microphone.GetPosition(deviceName) - samples.Length;
        if (micPos < 0)
            return 0f;

        micClip.GetData(samples, micPos);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];

        float loud = Mathf.Sqrt(sum / samples.Length);

        // soglia molto bassa → registra anche la voce debole
        if (loud < minimum)
            return 0f;

        // amplificazione per ottenere un valore utile
        return (loud - minimum) * sensitivity;
    }
}
