using UnityEngine;
using UnityEngine.InputSystem;

public class OpenDrawingBoard : MonoBehaviour
{
    public GameObject drawingBoard;  // assegna DrawingPanel qui
    public GameObject Canvas;
    public void Start()
    {
        Canvas.SetActive(false);
        drawingBoard.SetActive(false);
    }

    public void Update()
    {

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            print("hai cliccato il quadro bianco");
            Canvas.SetActive(true);
            drawingBoard.SetActive(true);
        }

    }
}
