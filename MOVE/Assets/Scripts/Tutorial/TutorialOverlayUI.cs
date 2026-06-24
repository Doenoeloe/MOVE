using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialOverlayUI : MonoBehaviour
{
    public static TutorialOverlayUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform promptBar;
    [SerializeField] private CanvasGroup   promptGroup;
    [SerializeField] private Transform     keyIconContainer;
    [SerializeField] private GameObject    keyBadgePrefab;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI dismissText;

    [Header("Animation")]
    [SerializeField] private float slideDistance = 60f;
    [SerializeField] private float animDuration  = 0.35f;

    private TutorialStep    _currentStep;
    private bool            _waitingForInput;
    private Vector2         _shownPosition;
    private Vector2         _hiddenPosition;
    private Coroutine       _animCoroutine;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _shownPosition  = promptBar.anchoredPosition;
        _hiddenPosition = _shownPosition + Vector2.down * slideDistance;

        promptBar.anchoredPosition = _hiddenPosition;
        promptGroup.alpha          = 0f;
    }

    public void Show(TutorialStep step)
    {
        _currentStep     = step;
        _waitingForInput = step.dismissOnInput;

        BuildKeyIcons(step.keyIcons);
        messageText.text = step.message;
        dismissText.text = step.dismissOnInput ? "Beweeg om door te gaan"
                         : step.autoDismiss    ? ""
                                               : "[ doorgaan ]";

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateIn());
    }

    public void Hide(System.Action onComplete = null)
    {
        _waitingForInput = false;
        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateOut(onComplete));
    }

    private void BuildKeyIcons(string[] keys)
    {
        foreach (Transform child in keyIconContainer)
            Destroy(child.gameObject);

        if (keys == null) return;

        foreach (string key in keys)
        {
            GameObject badge = Instantiate(keyBadgePrefab, keyIconContainer);
            badge.GetComponentInChildren<TextMeshProUGUI>().text = key;
        }
    }
    
    public void NotifyMovementInput()
    {
        if (!_waitingForInput) return;
        TutorialManager.Instance.Dismiss(_currentStep.id);
    }
    
    private IEnumerator AnimateIn()
    {
        promptBar.anchoredPosition = _hiddenPosition;
        promptGroup.alpha = 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / animDuration;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            promptBar.anchoredPosition = Vector2.LerpUnclamped(_hiddenPosition, _shownPosition, ease);
            promptGroup.alpha          = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t * 2f));
            yield return null;
        }

        promptBar.anchoredPosition = _shownPosition;
        promptGroup.alpha          = 1f;
    }

    private IEnumerator AnimateOut(System.Action onComplete = null)
    {
        float t = 0f;
        Vector2 startPos   = promptBar.anchoredPosition;
        float   startAlpha = promptGroup.alpha;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / animDuration;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            promptBar.anchoredPosition = Vector2.LerpUnclamped(startPos, _hiddenPosition, ease);
            promptGroup.alpha          = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(t * 2f));
            yield return null;
        }

        promptBar.anchoredPosition = _hiddenPosition;
        promptGroup.alpha          = 0f;
        onComplete?.Invoke();
    }
}