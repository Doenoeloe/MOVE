using UnityEngine;

public interface IFinisher
{
    int FinisherThreshold { get; }
    void TriggerFinisher(Transform target);
}