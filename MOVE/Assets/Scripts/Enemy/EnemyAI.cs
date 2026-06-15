using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyStateMachine))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(FactionComponent))]
public class EnemyAI : MonoBehaviour
{
    private EnemyStateMachine    _sm;
    private HealthComponent      _health;
    private IAttackSlotProvider  _slotProvider; // FIX: interface, not CombatArena directly
 
    public bool IsTargetable => _sm.IsTargetable;
    public bool IsHittable   => _sm.IsHittable;
    public EnemyStateMachine.AIState CurrentState => _sm.CurrentState;
 
    void Awake()
    {
        _sm     = GetComponent<EnemyStateMachine>();
        _health = GetComponent<HealthComponent>();
    }
 
    /// Called by CombatArena to inject itself — depends on interface not concrete class
    public void SetSlotProvider(IAttackSlotProvider provider) => _slotProvider = provider;
 
    public void TakeDamage(float amount, GameObject attacker = null)
        => _health.TakeDamage(amount, attacker);
 
    public void OnCountered()   => _sm.OnCountered();
    public void OnCounterMissed() { }
    public void DEBUG_ForceAttack() => _sm.DEBUG_ForceAttack();
}