using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zit op de PlayerRoot. Houdt bij welke cards actief zijn en
/// hoe vaak ze gestapeld zijn. Voert OnTick aan op alle actieve cards.
///
/// Cards communiceren via events — de handler exposeert ze zodat
/// cards zich kunnen abonneren zonder de concrete controller te kennen.
/// </summary>
public class SkillCardHandler : MonoBehaviour, ICharacterSwitchObserver
{
    // ── Events die cards kunnen abonneren ──────────────────────
    public event Action<Transform> OnAttackLanded;   // na succesvolle hit
    public event Action<float>     OnDamageTaken;    // schade ontvangen
    public event Action            OnKill;           // vijand gedood

    // ── Componenten ────────────────────────────────────────────
    public ComboTracker    ComboTracker    { get; private set; }
    public HealthComponent HealthComponent { get; private set; }
    public Animator        Animator        { get; private set; }
    public CharacterType   ActiveCharacter { get; private set; }

    // ── State ──────────────────────────────────────────────────
    // card → huidig aantal stacks
    private readonly Dictionary<SkillCardSO, int> _stacks = new();

    // geordende lijst voor OnTick iteratie
    private readonly List<SkillCardSO> _activeCards = new();
    private ActiveSkillManager _activeSkillManager;
    
    public IReadOnlyDictionary<SkillCardSO, int> Stacks    => _stacks;
    public IReadOnlyList<SkillCardSO>            ActiveCards => _activeCards;
    
    // Geaggregeerde stat multipliers — cards passen deze aan
    public float DamageMultiplier   { get; set; } = 1f;
    public float CooldownMultiplier { get; set; } = 1f;
    public float RangeBonus         { get; set; } = 0f;
    
    public CharacterType[] characterTypeByIndex;

    void Awake()
    {
        ComboTracker    = GetComponent<ComboTracker>();
        HealthComponent = GetComponent<HealthComponent>();
        Animator        = GetComponent<Animator>();
        _activeSkillManager = GetComponent<ActiveSkillManager>();
        if (characterTypeByIndex != null && characterTypeByIndex.Length > 0)
            ActiveCharacter = characterTypeByIndex[0];
    }

    void Update()
    {
        float dt = Time.deltaTime;
        foreach (var card in _activeCards)
            card.OnTick(this, dt);
    }

    // ── Public API ─────────────────────────────────────────────

    /// <summary>
    /// Voeg een card toe (gepickt na level-up).
    /// Eerste pick → OnEquip. Volgende → OnStack.
    /// </summary>
    public void AddCard(SkillCardSO card)
    {
        // Active skills are routed to ActiveSkillManager by CardOfferUI before this is called
        // Here we just register the stacks normally
        if (_stacks.TryGetValue(card, out int current))
        {
            if (card.maxStacks >= 0 && current >= card.maxStacks)
            {
                Debug.LogWarning($"[SkillCardHandler] {card.cardName} is al op max stacks.");
                return;
            }
            _stacks[card] = current + 1;
            card.OnStack(this, _stacks[card]);
            Debug.Log($"[SkillCardHandler] {card.cardName} gestapeld → {_stacks[card]}×");
        }
        else
        {
            _stacks[card] = 1;
            _activeCards.Add(card);
            card.OnEquip(this);
            Debug.Log($"[SkillCardHandler] {card.cardName} uitgerust (1×).");
        }
    }

    public int GetStackCount(SkillCardSO card)
        => _stacks.TryGetValue(card, out int n) ? n : 0;

    public bool HasCard(SkillCardSO card) => _stacks.ContainsKey(card);

    /// <summary>Reset aan het einde van een run.</summary>
    public void ResetRun()
    {
        foreach (var card in _activeCards)
            card.OnRunEnd(this);

        _stacks.Clear();
        _activeCards.Clear();
        _activeSkillManager?.ResetRun();
        DamageMultiplier   = 1f;
        CooldownMultiplier = 1f;
        RangeBonus         = 0f;

        Debug.Log("[SkillCardHandler] Run gereset — alle cards gewist.");
    }

    public void SetActiveCharacter(CharacterType type) => ActiveCharacter = type;

    // ── Notification methods — roep aan vanuit controllers ─────

    public void NotifyAttackLanded(Transform target) => OnAttackLanded?.Invoke(target);
    public void NotifyDamageTaken(float amount)      => OnDamageTaken?.Invoke(amount);
    public void NotifyKill()                          => OnKill?.Invoke();
    public void OnCharacterSwitched(int previousIndex, int newIndex)
    {
        if (characterTypeByIndex == null || newIndex >= characterTypeByIndex.Length) return;
        ActiveCharacter = characterTypeByIndex[newIndex];
        Debug.Log($"[SkillCardHandler] ActiveCharacter → {ActiveCharacter}");
    }
}