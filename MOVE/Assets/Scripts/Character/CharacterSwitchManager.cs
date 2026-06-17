using System.Collections.Generic;
using UnityEngine;

public class CharacterSwitchManager : MonoBehaviour
{
    [Header("Characters")]
    public int activeIndex = 0;
    public int previousIndex = -1;
    
    [Header("Shared State")]
    public float health    = 100f;
    public int   comboCount = 0;
    // FIX: No InputScheme here — PlayerInputHandler owns all input
    private IAttacker[] _attackers;
    private IAttacker   _activeAttacker;
    private SharedCharacterState _sharedState;
    private readonly List<ICharacterSwitchObserver> _observers = new();
    
    void Awake()
    {
        _attackers = GetComponentsInChildren<IAttacker>(includeInactive: true);
        _sharedState = GetComponent<SharedCharacterState>();
        
        foreach (var obs in GetComponentsInChildren<ICharacterSwitchObserver>())
            RegisterObserver(obs);
        
        if (_sharedState == null)
            Debug.LogError("[SwitchManager] SharedCharacterState not found on PlayerRoot. " +
                           "Add it as a component.");
    }
    public void RegisterObserver(ICharacterSwitchObserver observer)
    {
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }
    public void UnregisterObserver(ICharacterSwitchObserver observer) =>
        _observers.Remove(observer);
    
    void Start()
    {
        if (_attackers.Length == 0) return;
 
        _activeAttacker = _attackers[0];
        previousIndex = -1;
        
        var go = (_activeAttacker as MonoBehaviour)?.gameObject;
        go?.SetActive(true);
        go?.GetComponent<CharacterBase>()?.OnActivated(this, _sharedState);
 
        // Deactivate all others
        for (int i = 1; i < _attackers.Length; i++)
            (_attackers[i] as MonoBehaviour)?.gameObject.SetActive(false);
    }

    public void SwitchTo(int index)
    {
        if (index == activeIndex) return;
        if (index < 0 || index >= _attackers.Length) return;
        
        previousIndex = activeIndex;
        
        var outgoingGO     = (_activeAttacker as MonoBehaviour)?.gameObject;
        var incomingGO     = (_attackers[index] as MonoBehaviour)?.gameObject;
        var outgoingWindow = outgoingGO?.GetComponent<CounterWindow>();
        var incomingWindow = incomingGO?.GetComponent<CounterWindow>();

        if (outgoingWindow != null && outgoingWindow.IsOpen && incomingWindow != null)
        {
            Transform pending = outgoingWindow.PendingAttacker;
            outgoingWindow.ForceClose();
            incomingWindow.Open(pending);
        }

        outgoingGO?.GetComponent<CharacterBase>()?.OnDeactivated();
        outgoingGO?.SetActive(false);

        activeIndex     = index;
        _activeAttacker = _attackers[index];
        incomingGO?.SetActive(true);

        incomingGO?.GetComponent<CharacterBase>()?.OnActivated(this, _sharedState);
        
        foreach (var obs in _observers)
            obs.OnCharacterSwitched(previousIndex, index);
        
    }

    public GameObject GetActiveCharacter() =>
        (_activeAttacker as MonoBehaviour)?.gameObject;
}