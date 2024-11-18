using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {
        // Update the position of the custom cursor to match the mouse position
        Vector2 cursorPos = Input.mousePosition;
        transform.position = cursorPos;
    }
}