using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager sharedInstance;

    [SerializeField] RectTransform courtainTransform;
    [SerializeField] float transitionDuration;
    [SerializeField] Ease animationCurve;
    [SerializeField] float outXPosition, inXPosition;

    private void Awake()
    {
        if (sharedInstance == null)
        {
            sharedInstance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        courtainTransform.localPosition = new Vector3(inXPosition, 0, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        outTransition();
    }

    void outTransition()
    {
        courtainTransform.DOMoveX(outXPosition, transitionDuration).SetEase(animationCurve);
    }

    void inTransition(int sceneIndex)
    {
        var inSecuence = DOTween.Sequence();
        inSecuence.Append(courtainTransform.DOLocalMoveX(inXPosition, transitionDuration).SetEase(animationCurve)).OnComplete(() =>
        {
            inSecuence.Kill(true);
            SceneManager.LoadScene(sceneIndex);
        });
    }

    public void LoadScene(int sceneIndex)
    {
        inTransition(sceneIndex);
    }

    IEnumerator changeSceneWithDelayBeforeExit(int sceneIndex)
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneIndex);

    }
}
