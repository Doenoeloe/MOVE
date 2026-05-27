using UnityEngine;

public class CombatArena : MonoBehaviour, IAttackSlotProvider
{
    [Header("Slot Settings")]
    public int maxConcurrentAttackers = 1;

    private EnemyAI             _currentAttacker;
    private PlayerCombatManager _playerCombat;
    private CounterWindow       _counterWindow;
    
    public static CombatArena Instance { get; private set; }

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
        {
            _playerCombat  = player.GetComponent<PlayerCombatManager>();
            _counterWindow = player.GetComponent<CounterWindow>();
        }
    }

    void OnEnable()
    {
        // Push ourselves to every enemy already placed in the scene.
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
            enemy.SetSlotProvider(this);
    }
    

    public bool IsSlotFree => _currentAttacker == null;
    
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
    
    public void RegisterEnemy(EnemyAI enemy)
    {
        if (enemy != null)
            enemy.SetSlotProvider(this);
    }
    

#if UNITY_EDITOR
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 240, 20),
            $"Arena slot: {(_currentAttacker == null ? "free" : _currentAttacker.name)}");
    }
#endif
}