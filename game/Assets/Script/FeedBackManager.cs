using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking; // Necessario per le richieste web
using System.Collections;
using System.Text;
using UnityEngine.InputSystem;

public class FeedbackManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject feedbackPanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI statusText;
    public Button sendButton;

    [Header("Settings")]
    // INCOLLA QUI L'URL COPIATO DA DISCORD
    [SerializeField] private string discordWebhookURL = "https://discord.com/api/webhooks/1464585897054961720/aszQnm3CqT9f9cUb4_1dJQpP4_FoyBNJgh7AcKaL9bTbj-INe03U3I4_tylQbmDnBVnP";

    void Start()
    {
        // Assicuriamoci che il pannello sia chiuso all'avvio
        //if (feedbackPanel != null) feedbackPanel.SetActive(false);
    }
    void Update()
    {  
    }

    // Chiamata dal bottone "Apri Feedback" nel menu
    public void OpenPanel()
    {
        feedbackPanel.SetActive(true);
        inputField.interactable = true;
        statusText.text = "";
        inputField.text = "";
        sendButton.interactable = true;
    }

    public void ClosePanel()
    {
        feedbackPanel.SetActive(false);
    }

    // Chiamata dal bottone "Invia"
    public void SendFeedback()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            statusText.text = "Scrivi qualcosa prima di inviare!";
            return;
        }

        StartCoroutine(PostToDiscord(inputField.text));
    }

    IEnumerator PostToDiscord(string message)
    {
        statusText.text = "Invio in corso...";
        sendButton.interactable = false; // Evita spam di click

        // Aggiungiamo qualche info utile per il debug (Versione gioco, Risoluzione, ecc)
        string extraInfo = $"\n\n*Ver: {Application.version} | Platform: {Application.platform}*";
        string finalMessage = message + extraInfo;

        // Costruiamo il JSON per Discord
        // Discord si aspetta un oggetto JSON con chiave "content"
        // Attenzione: bisogna fare l'escape delle virgolette se presenti nel messaggio, 
        // ma per semplicit� qui costruiamo una stringa JSON base.

        string jsonPayload = "{\"content\": \"" + EscapeJson(finalMessage) + "\"}";

        // Creiamo la richiesta POST
        using (UnityWebRequest request = new UnityWebRequest(discordWebhookURL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Feedback inviato!");
                statusText.text = "Grazie per il feedback!";
                statusText.color = Color.green;

               
                inputField.interactable = false;
            }
            else
            {
                Debug.LogError("Errore invio: " + request.error);
                statusText.text = "Errore di connessione. Riprova.";
                statusText.color = Color.red;
                sendButton.interactable = true;
            }
        }
    }

    // Funzione helper per evitare che virgolette nel testo rompano il JSON
    string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
    }
}