using System.Collections;
using UnityEngine;

public class PlayerController : Jumper
{
    public override void Update()
    {
        base.Update();
        ReadInput();
    }

    private void ReadInput()
    {
        Vector2 nextLogicalCoordinates = currentLogicalCoordinates;
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            // up right
            nextLogicalCoordinates.x -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            nextLogicalCoordinates.y -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            nextLogicalCoordinates.y += 1;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            nextLogicalCoordinates.x += 1;
        }
        if (nextLogicalCoordinates != currentLogicalCoordinates)
        {
            Jump(nextLogicalCoordinates);
        }
    }

    public override void Jump(Vector2 targetLogicalCoordinates)
    {
        if (this.currentPlatform) return;
        base.Jump(targetLogicalCoordinates);
    }
}
