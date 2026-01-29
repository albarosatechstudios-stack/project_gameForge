using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Il Tuo Player")]
    public GameObject player; // Trascina qui il Player dalla gerarchia

    [Header("Spawn di Default")]
    public Transform defaultSpawnPoint; // Dove nasce se non trova corrispondenze (es. prima volta)

    [System.Serializable]
    public struct SpawnSetup
    {
        public string sceneName; // Il nome della scena DA CUI provieni (es. "MainMenu")
        public Transform spawnPoint; // Il punto dove deve apparire
    }

    [Header("Lista Punti di Spawn")]
    public List<SpawnSetup> spawnPoints; // Riempi questa lista nell'Inspector

    void Start()
    {
        PosizionaPlayer();
    }

    void PosizionaPlayer()
    {
        string scenaPrecedente = GameManager.lastScena;
        Transform puntoScelto = defaultSpawnPoint; // Partiamo dal default

        // Cerchiamo se c'è uno spawn specifico per la scena da cui veniamo
        foreach (var setup in spawnPoints)
        {
            if (setup.sceneName == scenaPrecedente)
            {
                puntoScelto = setup.spawnPoint;
                break;
            }
        }

        // --- APPLICAZIONE POSIZIONE ---
        if (puntoScelto != null && player != null)
        {
            // NOTA IMPORTANTE: Se usi un CharacterController, devi disattivarlo per teletrasportarlo!
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = puntoScelto.position;
            player.transform.rotation = puntoScelto.rotation;

            if (cc != null) cc.enabled = true;

            Debug.Log($"Player spawnato al punto: {puntoScelto.name} (Viene da: {scenaPrecedente})");
        }
    }
}