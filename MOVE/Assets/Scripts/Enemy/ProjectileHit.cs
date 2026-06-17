using UnityEngine;

/// <summary>
/// Zit op het projectiel-prefab. Deelt schade uit bij botsing met de speler.
/// </summary>
public class ProjectileHit : MonoBehaviour
{
    public float damage = 10f;
    public float lifetime = 5f;

    void Start() => Destroy(gameObject, lifetime);

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var switcher = other.GetComponent<CharacterSwitchManager>();
        var activeGO = switcher?.GetActiveCharacter();
        activeGO?.GetComponent<IHittable>()?.OnEnemyAttackLanded(transform);

        Destroy(gameObject);
    }
}