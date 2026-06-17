using System;
using System.Collections.Generic;
using UnityEngine;

public class CardDraftController : MonoBehaviour
{
    [Header("Card pool — sleep hier alle SkillCardSO assets in")]
    public List<SkillCardSO> cardPool = new();

    [Header("Draft size")]
    public int offersPerDraft = 3;

    [Header("Rarity weights")]
    public float weightCommon = 60f;
    public float weightRare   = 30f;
    public float weightEpic   = 10f;

    [Header("References")]
    public CardDraftUI draftUI;

    private XPSystem         _xp;
    private SkillCardHandler _handler;

    private readonly Queue<int> _pendingDrafts = new(); // one entry per level-up
    private bool _draftActive = false;

    public event Action<List<SkillCardSO>> OnDraftOffered;

    void Awake()
    {
        _xp      = GetComponent<XPSystem>();
        _handler = GetComponent<SkillCardHandler>();
    }

    void OnEnable()  => _xp.OnLevelUp += HandleLevelUp;
    void OnDisable() => _xp.OnLevelUp -= HandleLevelUp;

    void HandleLevelUp(int newLevel)
    {
        _pendingDrafts.Enqueue(newLevel);

        if (!_draftActive)
            ShowNextDraft();
    }

    void ShowNextDraft()
    {
        if (_pendingDrafts.Count == 0)
        {
            _draftActive = false;
            draftUI?.HidePanel();
            return;
        }

        _draftActive = true;
        _pendingDrafts.Dequeue(); // consume the level entry

        var offers = PickOffers();

        if (offers.Count == 0)
        {
            Debug.LogWarning("[CardDraftController] Geen geldige cards beschikbaar voor draft.");
            // Still need to drain the queue even if no cards are available
            ShowNextDraft();
            return;
        }

        OnDraftOffered?.Invoke(offers);
        draftUI?.ShowDraft(offers, OnCardPicked);
    }

    public void OnCardPicked(SkillCardSO card)
    {
        _handler.AddCard(card);
        Debug.Log($"[CardDraftController] Speler pikte: {card.cardName}");
        ShowNextDraft();
    }

    List<SkillCardSO> PickOffers()
    {
        var available = BuildAvailablePool();
        var result    = new List<SkillCardSO>();
        int attempts  = 0;

        while (result.Count < offersPerDraft && available.Count > 0 && attempts < 100)
        {
            attempts++;
            var picked = WeightedPick(available);
            if (picked == null) break;

            result.Add(picked);
            available.Remove(picked);
        }

        return result;
    }

    List<SkillCardSO> BuildAvailablePool()
    {
        var pool = new List<SkillCardSO>();

        foreach (var card in cardPool)
        {
            if (card == null) continue;

            if (card.allowedCharacter != CharacterType.Any &&
                card.allowedCharacter != _handler.ActiveCharacter)
                continue;

            int current = _handler.GetStackCount(card);
            if (card.maxStacks >= 0 && current >= card.maxStacks)
                continue;

            pool.Add(card);
        }

        return pool;
    }

    SkillCardSO WeightedPick(List<SkillCardSO> pool)
    {
        float totalWeight = 0f;
        foreach (var card in pool)
            totalWeight += RarityWeight(card.rarity);

        float roll       = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var card in pool)
        {
            cumulative += RarityWeight(card.rarity);
            if (roll <= cumulative) return card;
        }

        return pool[pool.Count - 1];
    }

    float RarityWeight(CardRarity rarity) => rarity switch
    {
        CardRarity.Common => weightCommon,
        CardRarity.Rare   => weightRare,
        CardRarity.Epic   => weightEpic,
        _                 => weightCommon
    };
}