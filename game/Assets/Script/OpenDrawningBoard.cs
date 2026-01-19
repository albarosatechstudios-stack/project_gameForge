using UnityEngine;
using UnityEngine.InputSystem;

public class OpenDrawingBoard : MonoBehaviour
{
    public GameObject drawingBoard;  // assegna DrawingPanel qui
    public GameObject Canvas;
    private bool inTrigger = false;
    public void Start()
    {
        Canvas.SetActive(false);
        drawingBoard.SetActive(false);
    }

    public void Update()
    {
        if (PauseMenu.GameIsPaused) return;
        if (inTrigger && Mouse.current.leftButton.wasPressedThisFrame)
        {
            print("hai cliccato il quadro bianco");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Canvas.SetActive(true);
            drawingBoard.SetActive(true);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        // Il player entra nel trigger
        if (other.CompareTag("Player"))
        {
            inTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Il player esce dal trigger
        if (other.CompareTag("Player"))
        {
            inTrigger = false;
        }
    }
}
