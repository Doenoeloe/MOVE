using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Combat/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Detection")]
    public float aggroRange  = 10f;
    public float attackRange = 1.8f;
 
    [Header("Timing")]
    public float telegraphDuration = 0.6f;
    public float attackDuration    = 0.4f;
    public float recoverDuration   = 1.0f;
    public float staggerDuration   = 1.2f;
 
    [Header("Health")]
    public float maxHealth  = 100f;
    public float baseDamage = 10f;
 
    [Header("Movement")]
    public float moveSpeed = 3.5f;
}

