using UnityEngine;

public abstract class CharacterBase : MonoBehaviour, IAttacker, IHittable
{
    protected CharacterSwitchManager manager;
    protected SharedCharacterState   SharedState;

    public virtual void OnActivated(CharacterSwitchManager mgr, SharedCharacterState state)
    {
        manager     = mgr;
        SharedState = state;
    }

    public virtual void OnDeactivated() { }

    public virtual void OnStagger() { }

    // IHittable — default behaviour routes through PlayerCombatManager.
    // Grappler overrides this to absorb and throw instead.
    public virtual void OnEnemyAttackLanded(Transform attacker)
    {
        GetComponentInParent<PlayerCombatManager>()?.OnTakeHit();
    }

    public abstract void Attack(Transform target);
    public abstract float AttackRange { get; }
}