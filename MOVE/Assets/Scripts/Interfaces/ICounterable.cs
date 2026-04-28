using UnityEngine;

public interface ICounterable
{
    bool CanCounter { get; }
    void Counter(Transform attacker);
}