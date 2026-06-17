using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "SkillCards/Brawler/Blink Strike")]
public class BlinkStrikeCard : ActiveSkillCardSO
{
    public float dashDistance = 8f;
    public float bonusDamage  = 0.5f;

    void Reset()
    {
        cardName         = "Blink Strike";
        description      = "Dash naar de dichtstbijzijnde vijand en versterk je volgende aanval.";
        cardColor        = new Color(0.2f, 0.6f, 1f);
        allowedCharacter = CharacterType.Brawler;
        rarity           = CardRarity.Rare;
        maxStacks        = 3;
        baseCooldown     = 8f;
    }

    public override void OnActivate(SkillCardHandler handler, int stackCount)
    {
        var nearest = FindNearest(handler.transform);
        if (nearest != null)
        {
            Vector3 dir       = (nearest.position - handler.transform.position).normalized;
            float   dist      = Vector3.Distance(handler.transform.position, nearest.position);
            Vector3 targetPos = handler.transform.position + dir * Mathf.Min(dashDistance * stackCount, dist - 1.2f);

            // Fix 1: bypass CharacterController so the teleport actually applies
            var cc = handler.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                handler.transform.position = targetPos;
                cc.enabled = true;
            }
            else
            {
                handler.transform.position = targetPos;
            }
        }

        // Fix 2: apply boost and hold it until the next attack lands
        float boost = 1f + bonusDamage * stackCount;
        handler.DamageMultiplier *= boost;
        handler.StartCoroutine(ResetAfterHit(handler, boost));
    }

    public override string GetStackDescription(int stackCount)
    {
        float boost = bonusDamage * stackCount * 100f;
        return $"Dash + volgende aanval +{boost:F0}% schade (stack {stackCount}×). CD: {baseCooldown}s";
    }

    Transform FindNearest(Transform origin)
    {
        var all    = Object.FindObjectsByType<EnemyStateMachine>(FindObjectsSortMode.None);
        Transform best  = null;
        float     bestD = float.MaxValue;

        foreach (var e in all)
        {
            float d = Vector3.Distance(origin.position, e.transform.position);
            if (d < bestD) { bestD = d; best = e.transform; }
        }

        return best;
    }

    // Waits until DamageMultiplier has been consumed (dropped back toward 1)
    // then removes whatever remainder is left from this boost.
    IEnumerator ResetAfterHit(SkillCardHandler handler, float boost)
    {
        float baseline = handler.DamageMultiplier / boost; // value before our boost
        float boosted  = handler.DamageMultiplier;

        // Wait until something external lowers the multiplier (i.e. an attack consumed it)
        while (handler.DamageMultiplier >= boosted - 0.01f)
            yield return null;

        // Clean up any leftover from our boost
        handler.DamageMultiplier /= boost;
    }
}