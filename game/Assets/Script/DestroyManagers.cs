using UnityEngine;

public class DestroyManagers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PauseMenu.GameIsPaused = false;
        if (MaestroManager.Instance != null)
        {
            MaestroManager.Instance.DistruggiManager();
        }
        if(TutorialManager.instance != null)
        {
            TutorialManager.instance.DistruggiManager();
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
