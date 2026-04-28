using UnityEngine;

/// Provides in-scene visual feedback for combat events when no animations exist yet.
/// Attach to PlayerRoot alongside PlayerCombatManager.
/// Draws hit flash, attack range sphere, and target line every frame in-editor.
[RequireComponent(typeof(PlayerCombatManager))]
[RequireComponent(typeof(TargetingSystem))]
public class CombatDebugVisualizer : MonoBehaviour
{
    [Header("Flash settings")]
    public Renderer playerRenderer;
    public Color    attackFlashColor  = new Color(1f, 0.85f, 0f, 1f);
    public Color    counterFlashColor = new Color(0f, 0.9f, 1f, 1f);
    public Color    hitFlashColor     = new Color(1f, 0.1f, 0.1f, 1f);
    public float    flashDuration     = 0.12f;

    [Header("Gizmo settings")]
    public float attackRangeGizmo = 2f;

    private Color           _baseColor;
    private float           _flashTimer;
    private Color           _flashTarget;
    private TargetingSystem _targeting;
    private ComboTracker    _combo;
    private CounterWindow   _counter;

    void Awake()
    {
        _targeting = GetComponent<TargetingSystem>();
        _combo     = GetComponent<ComboTracker>();
        _counter   = GetComponent<CounterWindow>();

        if (playerRenderer != null)
            _baseColor = playerRenderer.material.color;
    }

    void OnEnable()
    {
        var combat = GetComponent<PlayerCombatManager>();
        _combo.OnComboIncremented += _ => Flash(attackFlashColor);
        _combo.OnComboReset       += () => RestoreColor();
        _counter.OnWindowOpened   += _ => Flash(counterFlashColor);
        _counter.OnWindowResolved += () => Flash(counterFlashColor);
    }

    void OnDisable()
    {
        // Safe to leave — components may be destroyed together
    }

    void Update()
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= Time.deltaTime;
            if (_flashTimer <= 0f)
                RestoreColor();
        }
    }

    // Called externally by PlayerCombatManager.OnTakeHit via SendMessage or direct ref
    public void FlashHit() => Flash(hitFlashColor);

    void Flash(Color c)
    {
        if (playerRenderer == null) return;
        playerRenderer.material.color = c;
        _flashTarget = c;
        _flashTimer  = flashDuration;
    }

    void RestoreColor()
    {
        if (playerRenderer != null)
            playerRenderer.material.color = _baseColor;
    }

    // ── Scene gizmos ───────────────────────────────────────────

    void OnDrawGizmos()
    {
        if (_targeting == null) return;

        // Attack range ring
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, attackRangeGizmo);

        // Line to current target
        if (_targeting.CurrentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up,
                            _targeting.CurrentTarget.position + Vector3.up);
            Gizmos.DrawWireSphere(_targeting.CurrentTarget.position + Vector3.up * 0.5f, 0.3f);
        }

        // Counter window indicator — yellow pulsing sphere
        if (_counter != null && _counter.IsOpen)
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.9f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.4f);
        }
    }
}