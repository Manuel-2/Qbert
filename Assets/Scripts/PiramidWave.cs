using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PiramidWave : MonoBehaviour
{
    [Header("Piramid Building")]
    [SerializeField] int piramidLevels;
    [SerializeField] Vector2 blockStep;
    [SerializeField] float offset;
    [SerializeField] GameObject cubePrebaf;

    [Header("wave animation")]
    [SerializeField] float LevelTimeOffsetInWave;
    [SerializeField] float yMovement;
    [SerializeField] Ease yMovementEase;
    [SerializeField] float duration;

    List<List<GameObject>> rowLists;

    private void Start()
    {
        rowLists = Builder.BuildPiramidMap(this.transform,piramidLevels,new Vector2(blockStep.x + offset,blockStep.y + offset),cubePrebaf);
        StartCoroutine("WaveAnimation");
    }

    IEnumerator WaveAnimation()
    {
        for (int rowLevel = 0; rowLevel < piramidLevels; rowLevel++)
        {
            foreach(GameObject block in rowLists[rowLevel])
            {
                block.transform.DOLocalMoveY(block.transform.localPosition.y + yMovement, duration).SetLoops(-1, LoopType.Yoyo).SetEase(yMovementEase);
                //block.transform.DOScale(0.9f, duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutCubic);
            }
            yield return new WaitForSeconds(LevelTimeOffsetInWave);
        }
        yield return new WaitForSeconds(0);
    }

    //IEnumerator LateralWaveAnimation()
    //{
    //    for (int i = piramidLevels - 1; i >= 0; i--)
    //    {
    //        for (int j = i; j > 0; j--)
    //        {
    //            int y = piramidLevels - 1 - i;
    //            int x = j;
    //        }
    //    }
    //}
}
