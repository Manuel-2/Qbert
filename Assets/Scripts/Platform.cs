using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField]
    private Ease movementEaseFuntion;
    public Transform floorPivot;

    [HideInInspector]
    public Vector2 loogicalCoordinates;
    [HideInInspector]
    

    public void ActivatePlatform()
    {
        Vector3 targetPosition = Builder.sharedInstance.ConvertLogicalCoordinates2GlobalPosition(new Vector2(-1, -1));
        targetPosition.y += GameManager.sharedInstance.platformYOffset;
        var time = (targetPosition - this.transform.position).magnitude / GameManager.sharedInstance.platformSpeed;
        transform.DOMove(targetPosition, time).SetEase(movementEaseFuntion).OnComplete(GameManager.sharedInstance.EndRainbowPlatform);
        //thing.Kill(true);
    }
}
