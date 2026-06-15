using UnityEngine;

public interface ISlotAnimator
{
    void AnimateSelect(Transform slot);
    void AnimateDeselect(Transform slot);
}