using System.Collections;
using UnityEngine;

public class PlayerController : Jumper
{
    private bool colided;

    private void Start()
    {
        colided = false;
    }

    public override void Update()
    {
        base.Update();
        ReadInput();
    }

    private void ReadInput()
    {
        Vector2 nextLogicalCoordinates = currentLogicalCoordinates;
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Q))
        {
            // up right
            nextLogicalCoordinates.x -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.W))
        {
            nextLogicalCoordinates.y -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.A))
        {
            nextLogicalCoordinates.y += 1;
        }
        else if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.S))
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (!colided)
            {
                colided = true;
                GameManager.sharedInstance.PlayerDied();
            }
        }
        else if (collision.gameObject.CompareTag("Troll"))
        {
            GameManager.sharedInstance.EnemyDied(collision.gameObject.GetComponent<Jumper>());
            Destroy(collision.gameObject);
        }
    }
}
