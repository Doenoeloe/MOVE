using UnityEngine;
using System;

/// Single source of truth for everything that survives a character switch.
/// Sits on PlayerRoot alongside all other components.
public class SharedCharacterState : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float Health { get; private set; }

    [Header("Stagger")]
    public bool  IsStaggered     { get; private set; }
    public float staggerDuration = 0.4f;
    private float _staggerTimer;

    public event Action<float> OnHealthChanged;
    public event Action        OnDeath;
    public event Action        OnStaggerEnter;
    public event Action        OnStaggerExit;

    void Awake() => Health = maxHealth;

    void Update()
    {
        if (!IsStaggered) return;
        _staggerTimer -= Time.deltaTime;
        if (_staggerTimer <= 0f)
        {
            IsStaggered = false;
            OnStaggerExit?.Invoke();
        }
    }

    public void TakeDamage(float amount)
    {
        if (Health <= 0f) return;
        Health = Mathf.Max(0f, Health - amount);
        OnHealthChanged?.Invoke(Health);
        if (Health <= 0f) OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        Health = Mathf.Min(maxHealth, Health + amount);
        OnHealthChanged?.Invoke(Health);
    }

    public void EnterStagger()
    {
        IsStaggered   = true;
        _staggerTimer = staggerDuration;
        OnStaggerEnter?.Invoke();
    }
}