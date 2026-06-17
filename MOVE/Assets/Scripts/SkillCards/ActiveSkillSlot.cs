using UnityEngine;

public class ActiveSkillSlot
{
    public ActiveSkillCardSO Card     { get; private set; }
    public KeyCode           BoundKey { get; private set; }
    public float             Cooldown { get; private set; }
    public bool              IsReady  => Cooldown <= 0f;

    public ActiveSkillSlot(ActiveSkillCardSO card, KeyCode boundKey)
    {
        Card     = card;
        BoundKey = boundKey;
        Cooldown = 0f;
    }

    public void Tick(float dt)
    {
        if (Cooldown > 0f)
        {
            Cooldown -= dt;
            if (Cooldown < 0f) Cooldown = 0f;
        }
    }

    public void Activate(SkillCardHandler handler, int stackCount)
    {
        Debug.Log($"[ActiveSkillSlot] Activate called. Cooldown: {Cooldown} IsReady: {IsReady}");
        if (!IsReady) return;

        Card.OnActivate(handler, stackCount);

        float multiplier = Mathf.Max(0.1f, handler.CooldownMultiplier);
        Cooldown = Card.baseCooldown * multiplier;

        Debug.Log($"[ActiveSkillSlot] {Card.cardName} activated. CD set to: {Cooldown:F1}s");
    }

    public void ResetCooldown() => Cooldown = 0f;
}