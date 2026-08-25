using System.Collections;
using UnityEngine;
using TMPro;

public class QuestHUD : MonoBehaviour
{
    public static QuestHUD Instance { get; private set; }

    [SerializeField] private CanvasGroup  group;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private float animDuration = 0.3f;

    private Coroutine _anim;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        group.alpha = 0f;
    }

    public void Show(QuestObjective objective)
    {
        label.text = $"▶  {objective.description}";
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Fade(1f));
    }

    public void Hide()
    {
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / animDuration;
            group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t));
            yield return null;
        }
        group.alpha = target;
    }
}