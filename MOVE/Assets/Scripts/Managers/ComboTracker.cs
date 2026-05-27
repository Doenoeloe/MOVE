using UnityEngine;
using System;

public class ComboTracker : MonoBehaviour
{
    [Header("Settings")]
    public float resetTime = 1.8f;
    public int finisherThreshold = 8;

    public int Count { get; private set; }
    public bool FinisherAvailable => Count >= finisherThreshold;
    
    public event Action<int> OnComboIncremented;
    public event Action    OnComboReset;
    public event Action    OnFinisherUnlocked;
    
    private int _activeStaggerCount = 0;

    public void RegisterStagger()  => _activeStaggerCount++;
    public void ReleaseStagger()   => _activeStaggerCount = Mathf.Max(0, _activeStaggerCount - 1);

    private float _timer;

    void Update()
    {
        if (Count == 0) return;
        if (_activeStaggerCount > 0) return; // freeze while ANY enemy you hit is staggered

        _timer -= Time.deltaTime;
        if (_timer <= 0f) Reset();
    }

    public void RegisterHit()
    {
        Count++;
        _timer = resetTime;

        OnComboIncremented?.Invoke(Count);

        if (Count == finisherThreshold)
            OnFinisherUnlocked?.Invoke();
    }

    public void Reset()
    {
        Count = 0;
        _timer = 0f;
        OnComboReset?.Invoke();
    }
}