using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simpele XP-balk voor in de HUD.
/// Koppel aan de XPSystem op de PlayerRoot.
/// </summary>
public class XPBarUI : MonoBehaviour
{
    public Slider   xpSlider;
    public TMP_Text levelText;
    public TMP_Text xpText;

    private XPSystem _xp;

    void Start()
    {
        // Zoek de XPSystem op de player
        _xp = FindFirstObjectByType<XPSystem>();
        if (_xp == null)
        {
            Debug.LogError("[XPBarUI] Geen XPSystem gevonden in de scène.");
            return;
        }

        _xp.OnXPChanged += _ => Refresh();
        _xp.OnLevelUp   += _ => Refresh();
        Refresh();
    }

    void Refresh()
    {
        if (_xp == null) return;

        if (xpSlider  != null) xpSlider.value   = _xp.GetProgressNormalized();
        if (levelText != null) levelText.text    = $"Lvl {_xp.Level}";
        if (xpText    != null) xpText.text       = $"{_xp.CurrentXP:F0} / {_xp.XPToNext:F0}";
    }
}