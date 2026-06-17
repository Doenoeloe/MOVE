using UnityEngine;

/// <summary>
/// Preset voor de shield enemy. Langzamer, hogere stagger.
/// Gebruik op: ShieldEnemy prefab (heeft ook ArmorComponent + ParrySignal)
/// </summary>
[CreateAssetMenu(fileName = "ShieldEnemyData", menuName = "Enemy/Shield Data")]
public class ShieldEnemyData : EnemyData
{
    // Voorgestelde waarden: moveSpeed=2.0, telegraphDuration=1.0, staggerDuration=0.3
}