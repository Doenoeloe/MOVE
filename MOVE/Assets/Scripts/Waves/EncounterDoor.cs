using System.Collections;
using UnityEngine;

public class EncounterDoor : MonoBehaviour, IEncounterGate
{
    [Header("Animatie")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _lockedTrigger = "Close";
    [SerializeField] private string _unlockedTrigger = "Open";

    [Header("Physical blocking (optioneel naast animatie)")]
    [SerializeField] private Collider _blockingCollider;

    public void Lock()
    {
        if (_animator != null) _animator.SetTrigger(_lockedTrigger);
        if (_blockingCollider != null) _blockingCollider.enabled = true;
    }

    public void Unlock()
    {
        if (_animator != null) _animator.SetTrigger(_unlockedTrigger);
        if (_blockingCollider != null) _blockingCollider.enabled = false;
        
    }
}