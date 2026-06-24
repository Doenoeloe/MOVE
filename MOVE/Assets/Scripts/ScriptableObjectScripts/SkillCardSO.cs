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

    public virtual void OnEquip(SkillCardHandler handler) { }

    public virtual void OnStack(SkillCardHandler handler, int newStackCount) { }

    public virtual void OnTick(SkillCardHandler handler, float deltaTime) { }

    public virtual void OnRunEnd(SkillCardHandler handler) { }

    public virtual string GetStackDescription(int stackCount) => description;
}

public enum CardRarity { Common, Rare, Epic }