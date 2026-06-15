using UnityEngine;

public class CharacterWheelAnimator : MonoBehaviour, ICharacterSwitchObserver
{
    [Tooltip("Assign one CharacterWheelSlotView per character, in the same order as _attackers.")]
    [SerializeField] private CharacterWheelSlotView[] _slots;

    void Start()
    {
        // Initialise first slot as selected
        if (_slots.Length > 0) _slots[0].Select();
        for (int i = 1; i < _slots.Length; i++) _slots[i].Deselect();
    }

    // ICharacterSwitchObserver
    public void OnCharacterSwitched(int previousIndex, int newIndex)
    {
        if (previousIndex >= 0 && previousIndex < _slots.Length)
            _slots[previousIndex].Deselect();

        if (newIndex >= 0 && newIndex < _slots.Length)
            _slots[newIndex].Select();
    }
}