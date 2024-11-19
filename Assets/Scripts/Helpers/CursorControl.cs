using UnityEngine;
public class CursorControl : MonoBehaviour
{
    public KeyCode lockToggleKey;
    bool cursorLocked = true;
    void Update()
    {
        if(Input.GetKeyDown(lockToggleKey))
        {
            cursorLocked = !cursorLocked;
        }

        if(cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}