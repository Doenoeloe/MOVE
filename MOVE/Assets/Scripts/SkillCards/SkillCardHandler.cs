using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillCardHandler : MonoBehaviour, ICharacterSwitchObserver
{
    public event Action<Transform> OnAttackLanded;
    public event Action<float>     OnDamageTaken;
    public event Action            OnKill;
    
    public ComboTracker    ComboTracker    { get; private set; }
    public HealthComponent HealthComponent { get; private set; }
    public Animator        Animator        { get; private set; }
    public CharacterType   ActiveCharacter { get; private set; }
    
    private readonly Dictionary<SkillCardSO, int> _stacks = new();

    private readonly List<SkillCardSO> _activeCards = new();
    private ActiveSkillManager _activeSkillManager;
    
    public IReadOnlyDictionary<SkillCardSO, int> Stacks    => _stacks;
    public IReadOnlyList<SkillCardSO>            ActiveCards => _activeCards;
    
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

    public void AddCard(SkillCardSO card)
    {
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