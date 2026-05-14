using UnityEngine;
using System.Collections;

public abstract class BasePanel : MonoBehaviour
{
    protected CanvasGroup canvasGroup;
    protected Canvas rootCanvas;
    public bool isOpen;

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponent<Canvas>();
    }

    protected virtual void Start()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        rootCanvas.enabled = false;
        isOpen = false;
    }

    public virtual void Open()
    {
        StopAllCoroutines();
        rootCanvas.enabled = true;
        isOpen = true;
        StartCoroutine(Fade(true));
    }

    public virtual void Close()
    {
        StopAllCoroutines();
        isOpen = false;

        StartCoroutine(Fade(false, () =>
        {
            rootCanvas.enabled = false;
        }));
    }

    private IEnumerator Fade(bool fadeIn, System.Action onComplete = null)
    {
        float duration = 0.1f;
        float targetAlpha = fadeIn ? 1 : 0;
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = fadeIn;
        canvasGroup.interactable = fadeIn;
        onComplete?.Invoke();
    }
}