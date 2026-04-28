using UnityEngine;

/// Ensures only one enemy attacks at a time.
/// Enemies call RequestAttack() — only one gets a true response at a time.
/// Once their attack sequence ends they call ReleaseAttackSlot().
public class CombatArena : MonoBehaviour
{
    private EnemyAI          _currentAttacker;
    private PlayerCombatManager _playerCombat;
    private CounterWindow    _counterWindow;

    void Awake()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerCombat  = player.GetComponent<PlayerCombatManager>();
            _counterWindow = player.GetComponent<CounterWindow>();
        }
    }

    /// Returns true if the enemy is granted the attack slot.
    public bool RequestAttack(EnemyAI enemy)
    {
        if (_currentAttacker != null) return false;

        _currentAttacker = enemy;
        _counterWindow?.Open(enemy.transform);
        return true;
    }

    public void  ReleaseAttackSlot(EnemyAI enemy)
    {
        if (_currentAttacker == enemy)
            _currentAttacker = null;
    }

    public bool IsSlotFree => _currentAttacker == null;
}