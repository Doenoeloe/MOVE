using UnityEngine;

/// <summary>
/// Basis data asset voor alle enemy types.
/// Maak per enemy type een aparte asset via:
///   Right-click in Project → Create → Enemy → [Type] Data
/// </summary>
[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Base Data")]
public class EnemyData : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed    = 3.5f;
    public float aggroRange   = 10f;
    public float attackRange  = 2f;

    [Header("Combat Timing")]
    public float telegraphDuration = 0.8f;
    public float attackDuration    = 0.4f;
    public float staggerDuration   = 0.6f;
    public float recoverDuration   = 1.0f;
    public bool TriggersCombatTutorial;

    [Header("Rewards")]
    public int xpReward = 20;
}