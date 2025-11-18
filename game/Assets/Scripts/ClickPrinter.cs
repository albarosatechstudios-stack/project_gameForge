using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickPrinter : MonoBehaviour

{
 


    private void Start()
    {
        {

            var components = GetComponentsInChildren<Component>();
            foreach (var c in components)
            {
                Debug.Log(c.GetType().Name);
            }

        }
    }


        void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
            print("ciao sono Monnalisa");
        }
        }
    }



