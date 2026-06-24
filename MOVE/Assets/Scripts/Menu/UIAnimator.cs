using System.Collections;
using UnityEngine;
using TMPro;

public class UIAnimator : MonoBehaviour
{
    IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        float t = 0f;
        cg.alpha = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator MoveIn(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        float t = 0f;
        rt.anchoredPosition = from;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = 1f - Mathf.Pow(1f - t, 3f); // EaseOutCubic
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, ease);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    IEnumerator ScaleIn(RectTransform rt, float from, float to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            // EaseOutBack — overshoots slightly like a crash-in
            float ease = EaseOutBack(t);
            float s = Mathf.LerpUnclamped(from, to, ease);
            rt.localScale = Vector3.one * s;
            yield return null;
        }
        rt.localScale = Vector3.one * to;
    }

    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}