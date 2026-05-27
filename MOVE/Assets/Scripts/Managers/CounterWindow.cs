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

    private Coroutine _windowRoutine;

    public void Open(Transform attacker)
    {
        if (_windowRoutine != null)
            StopCoroutine(_windowRoutine);

        PendingAttacker = attacker;
        IsOpen = true;
        OnWindowOpened?.Invoke(attacker);
        _windowRoutine = StartCoroutine(WindowRoutine());
    }

    public void Resolve()
    {
        if (!IsOpen) return;

        StopCoroutine(_windowRoutine);
        _windowRoutine = null;
        IsOpen = false;
        
        var attacker = PendingAttacker;
        OnWindowResolved?.Invoke();       // PCM reads PendingAttacker here — still valid
        PendingAttacker = null;           // clear after subscribers have read it

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
        attacker?.GetComponent<EnemyAI>()?.OnCounterMissed();
    }
}