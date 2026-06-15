using UnityEngine;
using System.Collections;

public class ParrySignal : MonoBehaviour
{
    [Header("Glow Target")]
    public Renderer weaponRenderer;       // fist, sword, etc.
    public int      materialIndex = 0;    // which material on the renderer

    [Header("Glow Settings")]
    public Color  glowColor        = new Color(1f, 0.6f, 0f, 1f); // orange
    public Color  perfectGlowColor = new Color(1f, 0.1f, 0.1f, 1f); // red near end
    public float  glowIntensity    = 3f;
    public float  pulseDuration    = 0.6f; // should match telegraphDuration
    public float  perfectThreshold = 0.3f; // last N seconds = perfect window

    // Read by CounterWindow to know if we're in perfect window
    public bool IsPerfectWindow { get; private set; }

    private Material  _glowMat;
    private Color     _baseEmission;
    private Coroutine _glowRoutine;

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        if (weaponRenderer == null)
        {
            Debug.LogWarning($"[ParrySignal] No weaponRenderer assigned on {name}. " +
                             "Assign the fist or weapon mesh renderer in the Inspector.");
            return;
        }

        // Instance the material so we don't affect other enemies
        _glowMat      = weaponRenderer.materials[materialIndex];
        _baseEmission = _glowMat.GetColor(EmissionColorID);
    }

    /// Called by EnemyStateMachine when entering Telegraph
    public void TriggerGlow(float duration)
    {
        if (_glowMat == null) return;

        if (_glowRoutine != null)
            StopCoroutine(_glowRoutine);

        IsPerfectWindow = false;
        _glowRoutine    = StartCoroutine(GlowRoutine(duration));
    }

    /// Called when the window closes (parried, missed, or enemy staggered)
    public void CancelGlow()
    {
        if (_glowRoutine != null)
        {
            StopCoroutine(_glowRoutine);
            _glowRoutine = null;
        }

        IsPerfectWindow = false;
        SetEmission(Color.black);
    }

    IEnumerator GlowRoutine(float duration)
    {
        float elapsed   = 0f;
        float remaining = duration;

        // Enable emission
        _glowMat.EnableKeyword("_EMISSION");

        while (elapsed < duration)
        {
            remaining = duration - elapsed;

            // Switch to red in the perfect window
            IsPerfectWindow = remaining <= perfectThreshold;
            Color targetColor = IsPerfectWindow ? perfectGlowColor : glowColor;

            // Pulse: sine wave so it breathes rather than staying flat
            float pulse      = 0.7f + 0.3f * Mathf.Sin(elapsed * 12f);
            Color emission   = targetColor * (glowIntensity * pulse);
            SetEmission(emission);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetEmission(Color.black);
        IsPerfectWindow = false;
        _glowRoutine    = null;
    }

    void SetEmission(Color c)
    {
        if (_glowMat != null)
            _glowMat.SetColor(EmissionColorID, c);
    }

    void OnDisable() => CancelGlow();
}