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
        rowLists = Builder.BuildPiramidMap(this.transform, piramidLevels, new Vector2(blockStep.x + offset, blockStep.y + offset), cubePrebaf);
        StartCoroutine("LateralWaveAnimation");
    }

    IEnumerator WaveAnimation()
    {
        yield return new WaitForSeconds(duration);
        for (int rowLevel = 0; rowLevel < piramidLevels; rowLevel++)
        {
            foreach (GameObject block in rowLists[rowLevel])
            {
                block.transform.DOLocalMoveY(block.transform.localPosition.y + yMovement, duration).SetLoops(-1, LoopType.Yoyo).SetEase(yMovementEase);
                block.transform.DOScale(0.9f, duration).SetLoops(-1, LoopType.Yoyo).SetEase(yMovementEase);
            }
            yield return new WaitForSeconds(LevelTimeOffsetInWave);
        }
    }

    IEnumerator LateralWaveAnimation()
    {
        yield return new WaitForSeconds(duration);
        for (int y = piramidLevels - 1; y >= 0; y--)
        {
            for (int x = 0; x <= piramidLevels - 1 - y; x++)
            {
                Transform block = this.transform.Find($"{x},{y}");
                if (block != null)
                {
                    block.DOLocalMoveY(block.transform.localPosition.y + yMovement, duration).SetLoops(-1, LoopType.Yoyo).SetEase(yMovementEase);

                }
                else
                {
                    Debug.LogError("can not find a cube in the piramid to animate");
                }
            }
            yield return new WaitForSeconds(LevelTimeOffsetInWave);
        }
    }
}
