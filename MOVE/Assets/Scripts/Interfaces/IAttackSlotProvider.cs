using UnityEngine;

public interface IAttackSlotProvider
{
    bool RequestAttack(EnemyAI enemy);
    
    void ReleaseAttackSlot(EnemyAI enemy);
    
    void RegisterEnemy(EnemyAI enemy);
}