using System;
using UnityEngine;

public class XPSystem : MonoBehaviour
{
    [Header("XP curve")]
    [Tooltip("XP nodig voor level 1→2. Elke volgende level × xpScaling.")]
    public float baseXP     = 100f;
    public float xpScaling  = 1.25f; // exponentieel — aanpassen naar smaak
    public int   maxLevel   = 20;

    public int   Level    { get; private set; } = 1;
    public float CurrentXP{ get; private set; } = 0f;
    public float XPToNext => XPForLevel(Level);

    public event Action<int>   OnLevelUp;          // (newLevel)
    public event Action<float> OnXPChanged;        // (currentXP)

    // ── Public API ─────────────────────────────────────────────

    public void AddXP(float amount)
    {
        if (Level >= maxLevel) return;

        CurrentXP += amount;
        OnXPChanged?.Invoke(CurrentXP);

        // Loop voor het geval dat één kill meerdere levels oplevert
        while (CurrentXP >= XPToNext && Level < maxLevel)
        {
            CurrentXP -= XPToNext;
            Level++;
            Debug.Log($"[XPSystem] Level up! → {Level}");
            OnLevelUp?.Invoke(Level);
        }
    }

    public float GetProgressNormalized()
        => Mathf.Clamp01(CurrentXP / XPToNext);

    // ── Internal ───────────────────────────────────────────────

    float XPForLevel(int level)
        => Mathf.Round(baseXP * Mathf.Pow(xpScaling, level - 1));
}