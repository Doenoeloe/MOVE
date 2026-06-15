using UnityEngine;

public class FactionComponent : MonoBehaviour
{
    public enum Faction { Player, Enemy, Neutral }
 
    public Faction faction = Faction.Enemy;
    
    public bool IsEnemyOf(Faction other)
    {
        if (faction == Faction.Neutral || other == Faction.Neutral)
            return false;
        return faction != other;
    }
 
    /// Shorthand voor de meest gebruikte check
    public bool IsEnemy  => faction == Faction.Enemy;
    public bool IsPlayer => faction == Faction.Player;
}