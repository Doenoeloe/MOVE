using UnityEngine;

public class CombatArena : MonoBehaviour, IAttackSlotProvider
{
    [Header("Slot Settings")]
    public int maxConcurrentAttackers = 1; // reserved for future multi-slot support
 
    public static CombatArena Instance { get; private set; }
 
    private EnemyAI             _currentAttacker;
    private CounterWindow       _counterWindow;
 
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CombatArena] Duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
 
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _counterWindow = player.GetComponent<CounterWindow>();
    }
 
    void OnEnable()
    {
        // Register ourselves with every enemy already in the scene
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
            enemy.SetSlotProvider(this);
    }
 
    // ── IAttackSlotProvider ────────────────────────────────────
 
    public bool RequestAttack(EnemyAI enemy)
    {
        if (_currentAttacker != null) return false;
 
        _currentAttacker = enemy;
        _counterWindow?.Open(enemy.transform);
        return true;
    }
 
    public void ReleaseAttackSlot(EnemyAI enemy)
    {
        if (_currentAttacker == enemy)
            _currentAttacker = null;
    }
 
    // ── Public helpers ─────────────────────────────────────────
 
    public bool IsSlotFree => _currentAttacker == null;
 
    /// Called when a new enemy spawns at runtime
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (enemy != null) enemy.SetSlotProvider(this);
    }
 
#if UNITY_EDITOR
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 240, 20),
            $"Arena slot: {(_currentAttacker == null ? "free" : _currentAttacker.name)}");
    }
#endif
}
