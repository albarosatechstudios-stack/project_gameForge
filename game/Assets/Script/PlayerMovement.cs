using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Vector2 moveInput;

    private void Update()
    {
        // Legge WASD dal nuovo input system
        moveInput = Keyboard.current == null ? Vector2.zero : new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) +
            (Keyboard.current.aKey.isPressed ? -1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) +
            (Keyboard.current.sKey.isPressed ? -1 : 0)
        );

        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}

