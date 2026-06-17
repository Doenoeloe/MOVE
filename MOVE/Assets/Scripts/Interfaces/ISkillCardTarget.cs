using UnityEngine;

public interface ISkillCardTarget
{
    CharacterType CharacterType { get; }
    GameObject    GameObject    { get; }
    Transform     Transform     { get; }

    // Stats — cards passen deze aan via multipliers
    float DamageMultiplier   { get; set; }
    float CooldownMultiplier { get; set; }
    float RangeBonus         { get; set; }
    int   MaxComboBonus      { get; set; }

    // Events die cards kunnen abonneren
    event System.Action<Transform> OnAttackLanded;   // na elke succesvolle hit
    event System.Action            OnActiveTriggered; // speler drukt de ability-knop
    event System.Action<float>     OnDamageTaken;

    // Utility
    ComboTracker   ComboTracker   { get; }
    HealthComponent HealthComponent { get; }
    Animator       Animator       { get; }
}