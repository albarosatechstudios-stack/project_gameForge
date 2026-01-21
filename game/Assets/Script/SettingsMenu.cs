using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown micDropdown;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;

    void Start()
    {
        // 1. Inizializza Grafica
        SetupMicrophoneList();

        // 2. Sincronizza i valori visivi con quelli del MenuController
        if (MenuController.instance != null)
        {
            // Imposta lo slider sul valore attuale del Manager
            float currentSens = MenuController.instance.mouseSensitivity;
            sensitivitySlider.value = currentSens;
            UpdateSensText(currentSens);

            // Imposta il dropdown sull'indice attuale del Manager
            int currentMic = MenuController.instance.microphoneIndex;
            // Controllo di sicurezza per evitare errori se l'indice è fuori range
            if (currentMic < micDropdown.options.Count)
                micDropdown.value = currentMic;
        }

        // 3. Aggiungi Listener (quando muovi slider/dropdown, avvisa il Manager)
        sensitivitySlider.onValueChanged.AddListener(OnSensChanged);
        micDropdown.onValueChanged.AddListener(OnMicChanged);
    }

    void SetupMicrophoneList()
    {
        string[] devices = Microphone.devices;
        micDropdown.ClearOptions();

        if (devices.Length == 0)
        {
            micDropdown.AddOptions(new List<string> { "Nessun Microfono" });
        }
        else
        {
            micDropdown.AddOptions(new List<string>(devices));
        }
    }

    // --- EVENTI ---

    // Chiamato quando muovi lo slider
    void OnSensChanged(float val)
    {
        UpdateSensText(val);

        // MANDA IL DATO AL MENUCONTROLLER
        if (MenuController.instance != null)
        {
            MenuController.instance.UpdateSensitivity(val);
        }
    }

    // Chiamato quando cambi microfono
    void OnMicChanged(int index)
    {
        // MANDA IL DATO AL MENUCONTROLLER
        if (MenuController.instance != null)
        {
            MenuController.instance.UpdateMicrophoneIndex(index);
        }
    }

    void UpdateSensText(float val)
    {
        if (sensitivityValueText != null) sensitivityValueText.text = val.ToString("F1");
    }
}