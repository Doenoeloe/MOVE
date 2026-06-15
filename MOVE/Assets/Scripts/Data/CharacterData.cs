using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Combat/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Combat")]
    public float attackCooldown    = 0.18f;
    public float baseDamage        = 10f;
    public int   maxComboLength    = 8;
    public int   finisherThreshold = 6;
 
    [Header("Combo")]
    public float comboResetTime    = 1.2f;
    public float minAttackSpeed    = 1.0f;
    public float maxAttackSpeed    = 1.6f;
 
    [Header("Hit Detection")]
    public float attackRange = 1.6f;
    public float hitRadius   = 0.4f;
}
