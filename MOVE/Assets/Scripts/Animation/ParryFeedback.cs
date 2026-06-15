using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CounterWindow))]
public class ParryFeedback : MonoBehaviour
{
    [Header("Slow Motion")]
    public float slowTimeScale    = 0.15f;  // how slow time gets
    public float slowDuration     = 0.25f;  // real-world seconds (unscaled)
    public float slowRecoverSpeed = 8f;     // how fast time returns to 1.0

    [Header("Screen Flash")]
    public Color  flashColor    = new Color(1f, 1f, 1f, 0.45f);
    public float  flashDuration = 0.12f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   normalParryClip;
    public AudioClip   perfectParryClip;
    
    public UnityEngine.UI.Image screenFlashImage;

    private CounterWindow _counter;
    private Coroutine     _slowRoutine;
    private Coroutine     _flashRoutine;

    void Awake()
    {
        _counter = GetComponent<CounterWindow>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        _counter.OnParrySuccess += HandleParrySuccess;
    }

    void OnDisable()
    {
        _counter.OnParrySuccess -= HandleParrySuccess;
    }

    void HandleParrySuccess(bool isPerfect)
    {
        if (isPerfect)
        {
            Debug.Log("[Parry] PERFECT parry — slow motion triggered.");
            TriggerSlowMotion();
            TriggerScreenFlash();
            PlaySound(perfectParryClip);
        }
        else
        {
            Debug.Log("[Parry] Normal parry.");
            PlaySound(normalParryClip);
        }
    }

    void TriggerSlowMotion()
    {
        if (_slowRoutine != null) StopCoroutine(_slowRoutine);
        _slowRoutine = StartCoroutine(SlowMotionRoutine());
    }

    IEnumerator SlowMotionRoutine()
    {
        Time.timeScale       = slowTimeScale;
        Time.fixedDeltaTime  = 0.02f * slowTimeScale;

        // Wait in real time, not game time
        float elapsed = 0f;
        while (elapsed < slowDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Smoothly return to normal speed
        while (Time.timeScale < 1f)
        {
            Time.timeScale      = Mathf.MoveTowards(
                Time.timeScale, 1f, slowRecoverSpeed * Time.unscaledDeltaTime);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            yield return null;
        }

        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;
        _slowRoutine        = null;
    }

    void TriggerScreenFlash()
    {
        if (screenFlashImage == null) return;
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        screenFlashImage.color = flashColor;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            var   c     = screenFlashImage.color;
            c.a         = alpha;
            screenFlashImage.color = c;
            elapsed    += Time.unscaledDeltaTime; // unscaled — plays during slow-mo
            yield return null;
        }

        var final = screenFlashImage.color;
        final.a   = 0f;
        screenFlashImage.color = final;
        _flashRoutine          = null;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }
}