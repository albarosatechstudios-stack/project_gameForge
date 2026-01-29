using UnityEngine;

public class DestroyManagers : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (MaestroManager.Instance != null)
        {
            MaestroManager.Instance.DistruggiManager();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
