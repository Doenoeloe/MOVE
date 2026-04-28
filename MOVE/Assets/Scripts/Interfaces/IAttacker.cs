using UnityEngine;


public interface IAttacker
{
    float AttackRange { get; }
    void Attack(Transform target);
}