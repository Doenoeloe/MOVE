using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;
using System.Collections;

public class MenuButton : MonoBehaviour, 
    IPointerEnterHandler, 
    IPointerExitHandler, 
    IPointerClickHandler
{
    [Header("Referenties")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private RectTransform underline;

    [Header("Kleuren")]
    [SerializeField] private Color normalColor  = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color hoverColor   = new Color(1f, 1f, 1f, 1.0f);

    [Header("Actie")]
    public UnityEvent onClick;

    private Coroutine _anim;
    private bool _hovered = false;

    void Start()
    {
        label.color = normalColor;
        underline.localScale = new Vector3(0f, 1f, 1f);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _hovered = true;
        Animate(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        _hovered = false;
        Animate(false);
    }

    public void OnPointerClick(PointerEventData e)
    {
        onClick?.Invoke();
    }

    void Animate(bool hovering)
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(HoverAnim(hovering));
    }

    IEnumerator HoverAnim(bool hovering)
    {
        float duration = 0.15f;
        float t = 0f;

        Color startColor     = label.color;
        Color targetColor    = hovering ? hoverColor : normalColor;

        float startScale     = underline.localScale.x;
        float targetScale    = hovering ? 1f : 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float ease = Mathf.SmoothStep(0f, 1f, t);

            label.color = Color.Lerp(startColor, targetColor, ease);
            underline.localScale = new Vector3(
                Mathf.Lerp(startScale, targetScale, ease),
                1f, 1f
            );

            yield return null;
        }

        label.color = targetColor;
        underline.localScale = new Vector3(targetScale, 1f, 1f);
    }
}