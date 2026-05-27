using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    public float Health { get; private set; }
    public bool  IsDead  => Health <= 0f;
    
    public event System.Action OnDied;
    
    public event System.Action<float> OnDamageTaken;

    void Awake() => Health = maxHealth;
    
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        Health -= amount;

        if (Health <= 0f)
        {
            Health = 0f;
            OnDied?.Invoke();
        }
        else
        {
            OnDamageTaken?.Invoke(Health);
        }
    }
}