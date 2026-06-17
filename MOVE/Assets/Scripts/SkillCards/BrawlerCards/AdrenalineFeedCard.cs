using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SkillCards/Brawler/Adrenaline Feed")]
public class AdrenalineFeedCard : SkillCardSO
{
    public float healBase      = 2f;
    public float healPerStack  = 1f;
 
    private Action<Transform> _handler;
 
    void Reset()
    {
        cardName         = "Adrenaline Feed";
        description      = "Elke hit herstelt 2 HP.";
        cardColor        = new Color(0.9f, 0.5f, 0.0f);
        allowedCharacter = CharacterType.Brawler;
        rarity           = CardRarity.Rare;
        maxStacks        = 4;
    }
 
    public override void OnEquip(SkillCardHandler handler)
    {
        _handler = _ => Heal(handler);
        handler.OnAttackLanded += _handler;
    }
 
    public override void OnStack(SkillCardHandler handler, int newStackCount) { }
 
    public override void OnRunEnd(SkillCardHandler handler)
    {
        if (_handler != null) handler.OnAttackLanded -= _handler;
    }
 
    public override string GetStackDescription(int stackCount)
    {
        float total = healBase + healPerStack * (stackCount - 1);
        return $"Elke hit herstelt {total:F0} HP (stack {stackCount}×).";
    }
 
    void Heal(SkillCardHandler handler)
    {
        int stacks     = handler.GetStackCount(this);
        float healAmt  = healBase + healPerStack * (stacks - 1);
        handler.HealthComponent?.Heal(healAmt);
    }
}