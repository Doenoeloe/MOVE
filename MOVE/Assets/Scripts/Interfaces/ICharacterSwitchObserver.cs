using UnityEngine;

public interface ICharacterSwitchObserver
{
    void OnCharacterSwitched(int previousIndex, int newIndex);
}
