using UnityEngine;

public class SoffioScript : MonoBehaviour
{
    public float sensitivity = 40f;
    public float minimum = 0.01f;
    public int indexMicrofono = 0; // FIX: Meglio partire da 0 invece che 5
    private AudioClip micClip;
    private string deviceName;
    private float[] samples = new float[256];

    void Start()
    {
        // Controllo preliminare se ci sono microfoni
        if (Microphone.devices.Length > 0)
        {
            MenuController.instance.OnMicrophoneChanged += UpdateSens;

            if (MenuController.instance != null)
            {
                indexMicrofono = MenuController.instance.microphoneIndex;
            }

            // --- FIX CRITICO: CONTROLLO INDICE ---
            // Se l'indice salvato è maggiore dei microfoni disponibili, resettalo a 0 (il primo disponibile)
            if (indexMicrofono >= Microphone.devices.Length)
            {
                Debug.LogWarning($"Indice microfono salvato ({indexMicrofono}) non valido. Reset a 0.");
                indexMicrofono = 0;

                // Opzionale: aggiorna anche il MenuController per correggere il dato salvato
                if (MenuController.instance != null)
                    MenuController.instance.UpdateMicrophoneIndex(0);
            }
            // -------------------------------------

            deviceName = Microphone.devices[indexMicrofono];

            try
            {
                micClip = Microphone.Start(deviceName, true, 1, 44100);
                Debug.Log("Microfono avviato: " + deviceName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Errore avvio microfono: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Nessun microfono rilevato. Disattivo funzionalità soffio.");
        }
    }

    void Update()
    {
        // Esegui solo se il microfono sta effettivamente registrando
        if (Microphone.IsRecording(deviceName))
        {
            float power = GetSoundStrength();
            Debug.Log("Intensità voce/soffio : " + power + " | "+ deviceName); // Commentato per pulizia console
        }
    }

    void UpdateSens(int newVal)
    {
        // 1. Controllo di sicurezza: l'indice esiste?
        if (newVal < 0 || newVal >= Microphone.devices.Length)
        {
            Debug.LogError($"Indice microfono {newVal} non valido. Dispositivi disponibili: {Microphone.devices.Length}");
            // Se l'indice è sbagliato, prova a forzare il default
            if (Microphone.devices.Length > 0) UpdateSens(0);
            return;
        }

        // 2. Fermiamo il microfono precedente se sta registrando
        if (Microphone.IsRecording(deviceName))
        {
            Microphone.End(deviceName);
        }

        // 3. Aggiorniamo i dati e riavviamo
        indexMicrofono = newVal;
        deviceName = Microphone.devices[indexMicrofono];

        // Riavvia il microfono
        micClip = Microphone.Start(deviceName, true, 1, 44100);

        // Aspettiamo che il microfono sia pronto (loop di sicurezza con timeout per evitare freeze)
        int timeout = 0;
        while (!(Microphone.GetPosition(deviceName) > 0) && timeout < 1000)
        {
            timeout++;
        }

        Debug.Log("Microfono cambiato e riavviato: " + deviceName);
    }

    public float GetSoundStrength()
    {
        if (micClip == null)
            return 0f;

        // Sicurezza aggiuntiva per evitare errori di lettura buffer
        int micPos = Microphone.GetPosition(deviceName) - samples.Length;
        if (micPos < 0)
            return 0f;

        micClip.GetData(samples, micPos);

        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];

        float loud = Mathf.Sqrt(sum / samples.Length);

        if (loud < minimum)
            return 0f;

        return (loud - minimum) * sensitivity;
    }

    private void OnDestroy()
    {
        if (MenuController.instance != null)
        {
            MenuController.instance.OnMicrophoneChanged -= UpdateSens;
        }

        // Buona norma: fermare il microfono quando l'oggetto viene distrutto
        if (Microphone.IsRecording(deviceName))
        {
            Microphone.End(deviceName);
        }
    }
}