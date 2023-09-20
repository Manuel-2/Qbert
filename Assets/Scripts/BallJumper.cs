using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallJumper : Jumper
{
    public override void Update()
    {
        base.Update();
        if(this.lerping == false)
        {
            bool coinFlip = Random.Range(0, 2) > 0;
            Vector2 logicalTarget = this.currentLogicalCoordinates;
            if (coinFlip)
            {
                logicalTarget.x++;
            }
            else
            {
                logicalTarget.y++;
            }
            this.Jump(logicalTarget);
        }
    }

    public override void Jump(Vector2 targetLogicalCoordinates)
    {
        if (!canJump) return;
        base.Jump(targetLogicalCoordinates);
    }
}
