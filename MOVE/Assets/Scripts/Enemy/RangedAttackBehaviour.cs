using UnityEngine;

/// <summary>
/// Spawnt een projectiel richting de speler wanneer aangeroepen door EnemyStateMachine.
/// Hergebruikt op: RangedEnemy, BossEnemy
/// </summary>
public class RangedAttackBehaviour : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform  firePoint;
    public float      projectileSpeed  = 12f;
    public float      projectileDamage = 10f;

    [Header("Spread")]
    [Tooltip("Willekeurige hoekafwijking in graden (0 = perfect nauwkeurig).")]
    public float spreadAngle = 0f;

    private Transform _player;

    void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    /// <summary>
    /// Aanroepen vanuit EnemyStateMachine.UpdateAttacking() in plaats van melee logica.
    /// </summary>
    public void FireAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null || _player == null) return;

        Vector3 dir = (_player.position + Vector3.up * 0.5f - firePoint.position).normalized;

        if (spreadAngle > 0f)
        {
            dir = Quaternion.Euler(
                Random.Range(-spreadAngle, spreadAngle),
                Random.Range(-spreadAngle, spreadAngle),
                0f) * dir;
        }

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));

        if (proj.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = dir * projectileSpeed;

        if (proj.TryGetComponent<ProjectileHit>(out var hit))
            hit.damage = projectileDamage;
    }
}