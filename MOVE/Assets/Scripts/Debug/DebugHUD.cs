using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugHUD : MonoBehaviour
{
    [Header("References (auto-found if blank)")]
    public PlayerCombatManager combatManager;
    public ComboTracker        comboTracker;
    public CounterWindow       counterWindow;
    public TargetingSystem     targeting;
    public CombatArena         arena;
    public CharacterSwitchManager switchManager;

    [Header("UI Text fields")]
    public TMP_Text comboText;
    public TMP_Text stateText;
    public TMP_Text targetText;
    public TMP_Text counterText;
    public TMP_Text characterText;
    public TMP_Text arenaText;

    [Header("Enemy Test Buttons")]
    public Button forceAttackButton;  // wired in Inspector to DEBUG_ForceAllAttack()

    void Start()
    {
        // Auto-find if not assigned
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (combatManager  == null) combatManager  = player.GetComponent<PlayerCombatManager>();
            if (comboTracker   == null) comboTracker   = player.GetComponent<ComboTracker>();
            if (counterWindow  == null) counterWindow  = player.GetComponent<CounterWindow>();
            if (targeting      == null) targeting      = player.GetComponent<TargetingSystem>();
            if (switchManager  == null) switchManager  = player.GetComponent<CharacterSwitchManager>();
        }

        if (arena == null) arena = FindObjectOfType<CombatArena>();

        if (forceAttackButton != null)
            forceAttackButton.onClick.AddListener(DEBUG_ForceAllAttack);
    }

    void Update()
    {
        if (comboTracker != null && comboText != null)
        {
            comboText.text = $"Combo: {comboTracker.Count}" +
                             (comboTracker.FinisherAvailable ? "  [FINISHER]" : "");
        }

        if (counterWindow != null && counterText != null)
        {
            counterText.text = counterWindow.IsOpen
                ? $"Counter window: OPEN  ({counterWindow.PendingAttacker?.name})"
                : "Counter window: —";
            counterText.color = counterWindow.IsOpen ? Color.yellow : Color.white;
        }

        if (targeting != null && targetText != null)
        {
            targetText.text = targeting.CurrentTarget != null
                ? $"Target: {targeting.CurrentTarget.name}"
                : "Target: none";
        }

        if (switchManager != null && characterText != null)
        {
            characterText.text = $"Character: {switchManager.activeIndex}";
        }

        if (arena != null && arenaText != null)
        {
            arenaText.text = arena.IsSlotFree
                ? "Arena slot: free"
                : "Arena slot: OCCUPIED";
        }

        // Enemy state summary
        if (stateText != null)
        {
            var enemies = FindObjectsOfType<EnemyAI>();
            var sb = new System.Text.StringBuilder();
            foreach (var e in enemies)
                sb.AppendLine($"{e.name}: {e.CurrentState}");
            stateText.text = sb.ToString();
        }
    }

    // ── Debug actions ──────────────────────────────────────────

    // Forces ALL nearby enemies to attempt an attack — useful to test queue
    public void DEBUG_ForceAllAttack()
    {
        foreach (var e in FindObjectsOfType<EnemyAI>())
            e.DEBUG_ForceAttack();
    }
}