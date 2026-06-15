using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
 
    [Header("Faction")]
    [Tooltip("Wordt automatisch gevonden als leeg gelaten")]
    public FactionComponent faction;
 
    public float Health   { get; private set; }
    public bool  IsDead   => Health <= 0f;
    public bool  IsAlive  => Health > 0f;
    public bool IsInvincible { get; private set; }
 
    public event Action<float, float> OnHealthChanged; // (newHealth, maxHealth)
    public event Action<GameObject>   OnDeath;         // passes de aanvaller
    public event Action               OnRevive;
 
    void Awake()
    {
        Health  = maxHealth;
        faction = faction ?? GetComponent<FactionComponent>();
    }
 
    /// Geef schade. Optionele attacker voor death events.
    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (IsDead || IsInvincible || amount <= 0f) return;
 
        Health = Mathf.Max(0f, Health - amount);
        OnHealthChanged?.Invoke(Health, maxHealth);
 
        if (IsDead)
            OnDeath?.Invoke(attacker);
    }
 
    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;
        Health = Mathf.Min(maxHealth, Health + amount);
        OnHealthChanged?.Invoke(Health, maxHealth);
    }
 
    public void Revive(float healthAmount = -1f)
    {
        Health = healthAmount < 0f ? maxHealth : Mathf.Min(healthAmount, maxHealth);
        OnRevive?.Invoke();
        OnHealthChanged?.Invoke(Health, maxHealth);
    }
 
    public void SetInvincible(bool value) => IsInvincible = value;
 
    public void SetMaxHealth(float newMax, bool healToFull = false)
    {
        maxHealth = newMax;
        if (healToFull) Health = maxHealth;
        OnHealthChanged?.Invoke(Health, maxHealth);
    }
}