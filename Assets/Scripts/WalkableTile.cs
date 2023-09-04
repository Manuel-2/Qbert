using System.Collections;
using UnityEngine;


public class WalkableTile : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    int _currentColor = 0;

    public int currentColor
    {
        get
        {
            return _currentColor;
        }
    }

    int[] _position = null;

    public int[] logicalPosition
    {
        get
        {
            return _position;
        }

        set
        {
            if (_position != null)
            {
                return;
            }
            else
            {
                _position = new int[] { value[0], value[1] };
            }
        }
    }

    public void stepIn()
    {
        //TODO: read the current color, and find what colors the level has
        Debug.Log($"the tile {this.gameObject.name} has been pressed");
    }
}
