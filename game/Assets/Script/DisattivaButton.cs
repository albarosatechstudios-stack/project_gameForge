using UnityEngine;

public class DisattivaButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (MaestroManager.Instance.faseAttuale == QuestMaestro.FineGioco)
        {
            gameObject.SetActive(false);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
