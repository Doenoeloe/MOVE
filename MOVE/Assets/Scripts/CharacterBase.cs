using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    protected CharacterSwitchManager manager;

    public virtual void OnActivated(CharacterSwitchManager mgr)
    {
        manager = mgr;
        // Play switch-in animation, SFX, etc.
    }

    public virtual void OnDeactivated()
    {
        // Interrupt any active attack, reset state
    }

    public abstract void Attack();
    public abstract void Counter();
}