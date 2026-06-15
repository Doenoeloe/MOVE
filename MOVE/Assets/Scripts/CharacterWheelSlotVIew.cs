using UnityEngine;

public class CharacterWheelSlotView : MonoBehaviour
{
    [SerializeField] private ScaleColorSlotAnimator _animator;
    [SerializeField] private RectTransform _wheelCenter;
    private ISlotAnimator _slotAnimator;
    private RectTransform _rect;
    private Vector2 _originPosition;

    void Awake()
    {
        _slotAnimator = _animator;
        _rect = GetComponent<RectTransform>();
        _originPosition  = _rect.anchoredPosition;
    }

    public void Select()
    {
        _slotAnimator?.AnimateSelect(transform);
        AnimatePosition(selected: true);
    }

    public void Deselect()
    {
        _slotAnimator?.AnimateDeselect(transform);
        AnimatePosition(selected: false);
    }

    private void AnimatePosition(bool selected)
    {
        // Direction away from wheel center in local UI space
        Vector2 dir    = (_rect.anchoredPosition - GetWheelCenterLocalPos()).normalized;
        float   offset = selected ? 12f : 0f; // tweak this value

        StartCoroutine(_animator.AnimateAnchoredPosition(
            _rect,
            _originPosition + dir * offset,
            0.2f
        ));
    }

    private Vector2 GetWheelCenterLocalPos()
    {
        if (_wheelCenter == null) return Vector2.zero;

        // Convert wheel center to the same local space as this slot
        var parent = _rect.parent as RectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, _wheelCenter.position),
            null,
            out localPoint
        );
        return localPoint;
    }
    
    
}