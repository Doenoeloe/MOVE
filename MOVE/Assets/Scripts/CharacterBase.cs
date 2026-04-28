using UnityEngine;

public abstract class CharacterBase : MonoBehaviour, IAttacker
{
    protected CharacterSwitchManager manager;

    public virtual void OnActivated(CharacterSwitchManager mgr)
    {
        manager = mgr;
    }

    public virtual void OnDeactivated()
    {
        // Called when switching AWAY from this character
    }

    // Called when the player takes a hit — separate from OnDeactivated
    public virtual void OnStagger()
    {
        // Override in each character to play hurt reaction
    }

    public abstract void Attack(Transform target);
    public abstract float AttackRange { get; }
}