using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScaleColorSlotAnimator : MonoBehaviour, ISlotAnimator
{
    [Header("Scale")]
    [SerializeField] private float _selectedScale   = 1.4f;
    [SerializeField] private float _deselectedScale = 1.0f;
    [SerializeField] private float _duration        = 0.2f;

    [Header("Color")]
    [SerializeField] private Color _selectedColor   = Color.white;
    [SerializeField] private Color _deselectedColor = new Color(1f, 1f, 1f, 0.4f);

    public void AnimateSelect(Transform slot)   => StartCoroutine(Animate(slot, _selectedScale,   _selectedColor));
    public void AnimateDeselect(Transform slot) => StartCoroutine(Animate(slot, _deselectedScale, _deselectedColor));

    private IEnumerator Animate(Transform slot, float targetScale, Color targetColor)
    {
        var img          = slot.GetComponent<Image>();
        var startScale   = slot.localScale;
        var endScale     = Vector3.one * targetScale;
        var startColor   = img != null ? img.color : Color.white;
        float elapsed    = 0f;

        while (elapsed < _duration)
        {
            float t        = elapsed / _duration;
            float smooth   = t * t * (3f - 2f * t); // smoothstep

            slot.localScale = Vector3.Lerp(startScale, endScale, smooth);
            if (img != null) img.color = Color.Lerp(startColor, targetColor, smooth);

            elapsed += Time.deltaTime;
            yield return null;
        }

        slot.localScale = endScale;
        if (img != null) img.color = targetColor;
    }
    public IEnumerator AnimateAnchoredPosition(RectTransform rect, Vector2 target, float duration)
    {
        Vector2 start   = rect.anchoredPosition;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float smooth = t * t * (3f - 2f * t);
            rect.anchoredPosition = Vector2.Lerp(start, target, smooth);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = target;
    }
}