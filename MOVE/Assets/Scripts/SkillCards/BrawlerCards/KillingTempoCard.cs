using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SkillCards/Brawler/Killing Tempo")]
public class KillingTempoCard : SkillCardSO
{
    public float bonusMultiplierBase  = 3f;
    public float bonusPerStack        = 1f;
 
    private bool   _empowered;
    private Action _killHandler;
    private Action<Transform> _hitHandler;
 
    void Reset()
    {
        cardName         = "Killing Tempo";
        description      = "Na een kill: volgende aanval doet 3× schade.";
        cardColor        = new Color(0.6f, 0.0f, 0.8f);
        allowedCharacter = CharacterType.Brawler;
        rarity           = CardRarity.Epic;
        maxStacks        = 3;
    }
 
    public override void OnEquip(SkillCardHandler handler)
    {
        _killHandler = () => _empowered = true;
        _hitHandler  = _ =>
        {
            if (!_empowered) return;
            _empowered = false;
 
            int   stacks = handler.GetStackCount(this);
            float bonus  = bonusMultiplierBase + bonusPerStack * (stacks - 1);
 
            // Tijdelijke damage spike — geldt voor de volgende hit via de multiplier
            handler.DamageMultiplier *= bonus;
            // Reset na één tick zodat het maar één aanval beïnvloedt
            handler.StartCoroutine(ResetAfterFrame(handler, bonus));
        };
 
        handler.OnKill         += _killHandler;
        handler.OnAttackLanded += _hitHandler;
    }
 
    public override void OnRunEnd(SkillCardHandler handler)
    {
        if (_killHandler != null) handler.OnKill         -= _killHandler;
        if (_hitHandler  != null) handler.OnAttackLanded -= _hitHandler;
    }
 
    public override string GetStackDescription(int stackCount)
    {
        float total = bonusMultiplierBase + bonusPerStack * (stackCount - 1);
        return $"Na een kill: volgende aanval ×{total:F0} schade (stack {stackCount}×).";
    }
 
    System.Collections.IEnumerator ResetAfterFrame(SkillCardHandler handler, float bonus)
    {
        yield return null; // wacht één frame
        handler.DamageMultiplier /= bonus;
    }
}