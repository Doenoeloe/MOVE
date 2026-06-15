using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class CharacterWheelUI : MonoBehaviour
{
    [Header("Slot Images (match order of _attackers)")]
    public Image[] slotImages;

    public Image centerHighlight;

    [Header("Animation Settings")]
    public float selectedScale   = 1.8f;
    public float deselectedScale = 1.0f;
    public float animDuration    = 0.2f;

    public Color selectedColor   = Color.white;
    public Color deselectedColor = new Color(1f, 1f, 1f, 0.4f);

    public void OnCharacterSwitched(int previousIndex, int newIndex)
    {
        // Deselect old
        AnimateSlot(previousIndex, false);

        // Select new
        AnimateSlot(newIndex, true);
    }

    private void AnimateSlot(int index, bool selected)
    {
        if (index < 0 || index >= slotImages.Length) return;

        var img    = slotImages[index];
        var target = selected ? selectedScale : deselectedScale;
        var color  = selected ? selectedColor : deselectedColor;

        StartCoroutine(ScaleSlot(img.transform, target, animDuration));
    }
    
    IEnumerator ScaleSlot(Transform t, float targetScale, float duration)
    {
        Vector3 start = t.localScale;
        Vector3 end   = Vector3.one * targetScale;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        t.localScale = end;
    }
}
