using UnityEngine;

public class RecallGameMan : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.SetVisitor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
