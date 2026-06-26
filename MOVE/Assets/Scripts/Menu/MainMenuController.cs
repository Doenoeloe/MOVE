using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private RectTransform titleRect;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private RectTransform[] menuButtons;

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // Hit flash — white overlay fades out instantly
        fadeOverlay.alpha = 1f;
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 0f, 0.4f));

        // Title crashes down from above
        StartCoroutine(FadeCanvasGroup(titleGroup, 1f, 0.6f));
        yield return StartCoroutine(MoveIn(titleRect,
            titleRect.anchoredPosition + Vector2.up * 80f,
            titleRect.anchoredPosition, 0.8f));

        // Buttons stagger in
        foreach (RectTransform btn in menuButtons)
        {
            StartCoroutine(SlideInFromLeft(btn, 0.4f));
            yield return new WaitForSeconds(0.08f);
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float target, float duration)
    {
        float start = cg.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        cg.alpha = target;
    }

    IEnumerator MoveIn(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, ease);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    IEnumerator SlideInFromLeft(RectTransform rt, float duration)
    {
        CanvasGroup cg = rt.GetComponent<CanvasGroup>();
        Vector2 target = rt.anchoredPosition;
        Vector2 start = target + Vector2.left * 40f;
        float t = 0f;

        if (cg != null) cg.alpha = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            rt.anchoredPosition = Vector2.Lerp(start, target, ease);
            if (cg != null) cg.alpha = Mathf.Lerp(0f, 1f, t * 2f);
            yield return null;
        }

        rt.anchoredPosition = target;
        if (cg != null) cg.alpha = 1f;
    }

    // Called by button OnClick events in the Inspector
    public void OnNewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        StartCoroutine(TransitionToScene("GameScene"));
    }

    public void OnQuit()
    {
        Application.Quit();
    }

    IEnumerator TransitionToScene(string sceneName)
    {
        yield return StartCoroutine(FadeCanvasGroup(fadeOverlay, 1f, 0.5f));
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}