using UnityEngine;

[CreateAssetMenu(menuName = "SkillCards/Brawler/Berserker Rush")]
public class BerserkerRushCard : SkillCardSO
{
    public float damagePerStack = 0.15f;
 
    void Reset()
    {
        cardName         = "Berserker Rush";
        description      = "+15% schade.";
        cardColor        = new Color(0.8f, 0.2f, 0.1f);
        allowedCharacter = CharacterType.Brawler;
        rarity           = CardRarity.Common;
        maxStacks        = 5;
    }
 
    public override void OnEquip(SkillCardHandler handler)
        => handler.DamageMultiplier *= (1f + damagePerStack);
 
    public override void OnStack(SkillCardHandler handler, int newStackCount)
        => handler.DamageMultiplier *= (1f + damagePerStack);
 
    public override string GetStackDescription(int stackCount)
        => $"+{damagePerStack * stackCount * 100f:F0}% schade (stack {stackCount}×).";
 
    public override void OnRunEnd(SkillCardHandler handler)
        => handler.DamageMultiplier = 1f; // reset wordt toch door handler gedaan
}