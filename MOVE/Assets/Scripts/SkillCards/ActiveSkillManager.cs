using System.Collections.Generic;
using UnityEngine;

public class ActiveSkillManager : MonoBehaviour
{
    [Header("Max slots")] 
    public int maxSlots = 2;

    private readonly List<ActiveSkillSlot> _slots = new();
    private SkillCardHandler _handler;

    void Awake()
    {
        _handler = GetComponent<SkillCardHandler>();
    }

    void FixedUpdate()
    {
        foreach (var slot in _slots)
            slot.Tick(Time.fixedUnscaledDeltaTime);
    }

    public void TriggerSlot(KeyCode key)
    {
        Debug.Log($"[ActiveSkillManager] TriggerSlot [{key}] — slots: {_slots.Count}");
        foreach (var slot in _slots)
        {
            Debug.Log($"  slot BoundKey: {slot.BoundKey} == {key} ? {slot.BoundKey == key}");
            if (slot.BoundKey == key)
            {
                slot.Activate(_handler, _handler.GetStackCount(slot.Card));
                return;
            }
        }
        Debug.LogWarning($"[ActiveSkillManager] No slot matched [{key}]");
    }

    public bool TryEquipActive(ActiveSkillCardSO card, KeyCode boundKey)
    {
        // Already slotted — stacking, no new slot needed
        foreach (var slot in _slots)
            if (slot.Card == card) return true;

        if (_slots.Count >= maxSlots)
        {
            Debug.LogWarning($"[ActiveSkillManager] Alle {maxSlots} slots vol.");
            return false;
        }

        _slots.Add(new ActiveSkillSlot(card, boundKey));
        Debug.Log($"[ActiveSkillManager] {card.cardName} bound to [{boundKey}]");
        return true;
    }

    public bool IsSlotOccupied(KeyCode key)
    {
        foreach (var slot in _slots)
            if (slot.BoundKey == key) return true;
        return false;
    }

    public void ResetRun() => _slots.Clear();

    public IReadOnlyList<ActiveSkillSlot> Slots => _slots;
}