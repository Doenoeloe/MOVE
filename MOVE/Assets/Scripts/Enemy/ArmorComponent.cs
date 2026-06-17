using UnityEngine;

/// <summary>
/// Absorbs a flat amount of damage before the remainder reaches HealthComponent.
/// Hergebruikt op: ShieldEnemy, BossEnemy
/// </summary>
[RequireComponent(typeof(HealthComponent))]
public class ArmorComponent : MonoBehaviour
{
    [Header("Armor Settings")]
    [Tooltip("Hoeveel schade per hit wordt geabsorbeerd.")]
    public float damageReduction = 5f;

    [Tooltip("Nadat dit veel totale schade door armor is gegaan, breekt het.")]
    public float armorDurability = 30f;

    private float _currentDurability;
    private bool  _isBroken;

    public bool IsBroken => _isBroken;

    void Awake() => _currentDurability = armorDurability;

    /// <summary>
    /// Vermindert inkomende schade. Geeft de werkelijke schade terug die nog
    /// doorgegeven moet worden aan HealthComponent.
    /// </summary>
    public float FilterDamage(float rawDamage)
    {
        if (_isBroken) return rawDamage;

        float reduced = Mathf.Max(0f, rawDamage - damageReduction);
        float absorbed = rawDamage - reduced;

        _currentDurability -= absorbed;
        if (_currentDurability <= 0f)
        {
            _isBroken = true;
            Debug.Log($"[ArmorComponent] {name}: Armor gebroken!");
        }

        return reduced;
    }

    public void RepairArmor()
    {
        _currentDurability = armorDurability;
        _isBroken = false;
    }
}