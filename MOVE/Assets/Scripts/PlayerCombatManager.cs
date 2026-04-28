using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    private TargetingSystem        _targeting;
    private ComboTracker           _combo;
    private CounterWindow          _counter;
    private Animator               _anim;
    private CharacterSwitchManager _switcher;

    void Awake()
    {
        _targeting = GetComponent<TargetingSystem>();
        _combo     = GetComponent<ComboTracker>();
        _counter   = GetComponent<CounterWindow>();
        _anim      = GetComponent<Animator>();
        _switcher  = GetComponent<CharacterSwitchManager>();
    }

    void OnEnable()
    {
        _counter.OnWindowOpened   += OnCounterWindowOpened;
        _counter.OnWindowResolved += OnCounterWindowResolved;
        _counter.OnWindowExpired  += OnCounterWindowExpired;

        _combo.OnComboIncremented += count =>
            Debug.Log($"[Combo] Hit registered — count now {count}");
        _combo.OnComboReset += () =>
            Debug.Log("[Combo] Reset");
        _combo.OnFinisherUnlocked += () =>
            Debug.Log("[Combo] FINISHER AVAILABLE");
    }

    void OnDisable()
    {
        _counter.OnWindowOpened   -= OnCounterWindowOpened;
        _counter.OnWindowResolved -= OnCounterWindowResolved;
        _counter.OnWindowExpired  -= OnCounterWindowExpired;
    }

    ICounterable ActiveCounterable =>
        _switcher?.GetActiveCharacter()?.GetComponent<ICounterable>();

    IFinisher ActiveFinisher =>
        _switcher?.GetActiveCharacter()?.GetComponent<IFinisher>();

    public void OnAttack()
    {
        Debug.Log("[Attack] OnAttack() called");

        var activeGO = _switcher?.GetActiveCharacter();
        if (activeGO == null)
        {
            Debug.LogError("[Attack] FAIL — GetActiveCharacter() returned null.");
            return;
        }

        var target = _targeting.AcquireTarget();
        if (target == null)
        {
            Debug.LogWarning($"[Attack] No target found. Detection radius = " +
                             $"{_targeting.detectionRadius}, " +
                             $"Enemy layer mask = {_targeting.enemyLayer.value}. " +
                             "Check the Enemy GameObject's Layer matches the mask.");
            return;
        }
        Debug.Log($"[Attack] Target acquired: {target.name}");

        var attacker = activeGO.GetComponent<IAttacker>();
        if (attacker == null)
        {
            Debug.LogError($"[Attack] FAIL — {activeGO.name} has no IAttacker component.");
            return;
        }

        attacker.Attack(target);
    }

    public void OnCounter()
    {
        if (!_counter.IsOpen) return;

        var counterable = ActiveCounterable;
        if (counterable == null || !counterable.CanCounter) return;

        _counter.Resolve();
    }

    public void OnFinisher()
    {
        if (!_combo.FinisherAvailable) return;

        var finisher = ActiveFinisher;
        if (finisher == null) return;

        var target = _targeting.AcquireTarget();
        if (target == null) return;

        finisher.TriggerFinisher(target);
        _combo.Reset();
    }

    public void OnHitLanded()
    {
        Debug.Log($"[Combo] OnHitLanded — registering hit. Count before: {_combo.Count}");
        _combo.RegisterHit();
    }

    public void OnTakeHit()
    {
        Debug.Log("[Player] OnTakeHit — combo reset, target cleared.");
        _combo.Reset();
        _targeting.ClearTarget();
        
        _switcher?.GetActiveCharacter()
                 ?.GetComponent<CharacterBase>()
                 ?.OnStagger();
    }

    void OnCounterWindowOpened(Transform attacker)
    {
        Debug.Log($"[Counter] Window opened — attacker: {attacker?.name}");
        _targeting.SetOverrideTarget(attacker);
    }

    void OnCounterWindowResolved()
    {
        Debug.Log("[Counter] Window resolved — player countered successfully.");
        var counterable = ActiveCounterable;
        var attacker    = _counter.PendingAttacker;
        counterable?.Counter(attacker);

        if (counterable == null)
            _combo.RegisterHit();

        _targeting.ClearOverrideTarget();
    }

    void OnCounterWindowExpired()
    {
        Debug.Log("[Counter] Window expired — player missed the counter.");
        _targeting.ClearOverrideTarget();
    }
}