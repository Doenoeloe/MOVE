using UnityEngine;

public abstract class ActiveSkillCardSO : SkillCardSO
{
    [Header("Active Skill")]
    public float baseCooldown = 8f;

    public abstract void OnActivate(SkillCardHandler handler, int stackCount);

    public override void OnEquip(SkillCardHandler handler) { }
    public override void OnStack(SkillCardHandler handler, int newStackCount) { }
    public override void OnRunEnd(SkillCardHandler handler) { }
    public override void OnTick(SkillCardHandler handler, float dt) { }
}