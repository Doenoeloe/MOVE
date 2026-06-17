using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Dunne facade die externe systemen (CombatArena, speler) loskoppelt van de interne
/// state machine. Hergebruikt op: MeleeEnemy, RangedEnemy, ShieldEnemy, BossEnemy
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStateMachine))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(FactionComponent))]
public class EnemyAI : MonoBehaviour
{
    private EnemyStateMachine   _sm;
    private HealthComponent     _health;
    private IAttackSlotProvider _slotProvider;

    public bool IsTargetable => _sm.IsTargetable;
    public bool IsHittable   => _sm.IsHittable;
    public EnemyStateMachine.AIState CurrentState => _sm.CurrentState;

    void Awake()
    {
        _sm     = GetComponent<EnemyStateMachine>();
        _health = GetComponent<HealthComponent>();
    }

    public void SetSlotProvider(IAttackSlotProvider provider) => _slotProvider = provider;

    /// <summary>
    /// Schade loopt via de state machine zodat ArmorComponent kan filteren.
    /// </summary>
    public void TakeDamage(float amount, GameObject attacker = null)
    {
        float filtered = _sm.FilterDamage(amount);
        _health.TakeDamage(filtered, attacker);
    }

    public void OnCountered()      => _sm.OnCountered();
    public void OnCounterMissed()  { }
    public void DEBUG_ForceAttack() => _sm.DEBUG_ForceAttack();
}