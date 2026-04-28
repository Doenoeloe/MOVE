using UnityEngine;

public class CharacterSwitchManager : MonoBehaviour
{
    [Header("Characters")]
    public int activeIndex = 0;

    [Header("Shared State")]
    public float health    = 100f;
    public int   comboCount = 0;

    // FIX: No InputScheme here — PlayerInputHandler owns all input
    private IAttacker[] _attackers;
    private IAttacker   _activeAttacker;

    void Awake()
    {
        _attackers = GetComponentsInChildren<IAttacker>(includeInactive: true);
    }

    void Start()
    {
        if (_attackers.Length == 0) return;
 
        _activeAttacker = _attackers[0];
        var go = (_activeAttacker as MonoBehaviour)?.gameObject;
        go?.SetActive(true);
        go?.GetComponent<CharacterBase>()?.OnActivated(this);
 
        // Deactivate all others
        for (int i = 1; i < _attackers.Length; i++)
            (_attackers[i] as MonoBehaviour)?.gameObject.SetActive(false);
    }

    public void SwitchTo(int index)
    {
        if (index == activeIndex) return;
        if (index < 0 || index >= _attackers.Length) return;

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

        incomingGO?.GetComponent<CharacterBase>()?.OnActivated(this);
    }

    public GameObject GetActiveCharacter() =>
        (_activeAttacker as MonoBehaviour)?.gameObject;
}