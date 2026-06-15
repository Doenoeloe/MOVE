using UnityEngine;
using System;
using System.Collections;

public class CounterWindow : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.4f;

    public bool IsOpen { get; private set; }
    public Transform PendingAttacker { get; private set; }

    public event Action<Transform> OnWindowOpened;
    public event Action            OnWindowResolved;
    public event Action            OnWindowExpired;

    // Fires on successful parry — bool is true if it was a perfect parry
    public event Action<bool> OnParrySuccess;

    [Header("Perfect Parry")]
    public float perfectWindowDuration = 0.3f; // first N seconds = perfect window

    private Coroutine _windowRoutine;
    private float     _windowStartTime;

    public void Open(Transform attacker)
    {
        if (_windowRoutine != null)
            StopCoroutine(_windowRoutine);

        PendingAttacker  = attacker;
        IsOpen           = true;
        _windowStartTime = Time.time;

        // Trigger glow on the attacker's weapon
        attacker?.GetComponent<ParrySignal>()?.TriggerGlow(duration);

        OnWindowOpened?.Invoke(attacker);
        _windowRoutine = StartCoroutine(WindowRoutine());
    }

    public void Resolve()
    {
        if (!IsOpen) return;

        StopCoroutine(_windowRoutine);
        _windowRoutine = null;
        IsOpen = false;
        
        var attacker   = PendingAttacker;
        bool isPerfect = (Time.time - _windowStartTime) <= perfectWindowDuration;

        OnWindowResolved?.Invoke();
        OnParrySuccess?.Invoke(isPerfect);
        PendingAttacker = null;

        // Cancel glow immediately on successful parry
        attacker?.GetComponent<ParrySignal>()?.CancelGlow();
        attacker?.GetComponent<EnemyAI>()?.OnCountered();
    }

    public void ForceClose()
    {
        if (!IsOpen) return;

        if (_windowRoutine != null)
        {
            StopCoroutine(_windowRoutine);
            _windowRoutine = null;
        }

        IsOpen = false;
        PendingAttacker = null;
    }

    IEnumerator WindowRoutine()
    {
        yield return new WaitForSeconds(duration);

        IsOpen = false;
        var attacker = PendingAttacker;
        PendingAttacker = null;
        _windowRoutine = null;

        OnWindowExpired?.Invoke();
        attacker?.GetComponent<ParrySignal>()?.CancelGlow();
        attacker?.GetComponent<EnemyAI>()?.OnCounterMissed();
    }
}