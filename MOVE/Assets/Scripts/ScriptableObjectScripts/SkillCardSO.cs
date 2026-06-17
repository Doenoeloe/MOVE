using UnityEngine;

[CreateAssetMenu(menuName = "SkillCards/New Skill Card")]
public abstract class SkillCardSO : ScriptableObject
{
    [Header("Identity")]
    public string cardName   = "Unnamed Card";
    [TextArea(2, 4)]
    public string description = "";
    public Sprite icon;
    public Color  cardColor   = Color.white;

    [Header("Rarity")]
    public CardRarity rarity = CardRarity.Common;

    [Header("Restriction")]
    [Tooltip("Any = verschijnt voor alle characters")]
    public CharacterType allowedCharacter = CharacterType.Any;

    [Header("Stack behaviour")]
    [Tooltip("Hoeveel keer kan deze card maximaal gestapeld worden? -1 = oneindig")]
    public int maxStacks = -1;

    // ── Runtime hooks ──────────────────────────────────────────

    /// <summary>Eerste keer dat de card gepickt wordt.</summary>
    public virtual void OnEquip(SkillCardHandler handler) { }

    /// <summary>Elke volgende pick van dezelfde card (stack 2+).</summary>
    public virtual void OnStack(SkillCardHandler handler, int newStackCount) { }

    /// <summary>
    /// Aangeroepen elke frame terwijl de card actief is.
    /// Gebruik voor over-time effecten (regen, aura's).
    /// Vermijd zware logic hier — gebruik events waar mogelijk.
    /// </summary>
    public virtual void OnTick(SkillCardHandler handler, float deltaTime) { }

    /// <summary>Run voorbij (dood of reset). Ruim subscriptions op.</summary>
    public virtual void OnRunEnd(SkillCardHandler handler) { }

    /// <summary>Beschrijving voor stack N — override voor dynamische tekst.</summary>
    public virtual string GetStackDescription(int stackCount) => description;
}

public enum CardRarity { Common, Rare, Epic }