using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardOfferUI : MonoBehaviour
{
    [Header("Card UI")]
    public Image    background;
    public Image    iconImage;
    public TMP_Text rarityLabel;
    public TMP_Text cardNameText;
    public TMP_Text descriptionText;
    public TMP_Text stackLabel;
    public Button   selectButton;

    [Header("Slot Picker (active skills only)")]
    public GameObject        cardContent;      // parent of all normal card UI elements
    public GameObject        slotPickerPanel;  // shown instead when picking a slot
    public Button            slotQButton;
    public Button            slotEButton;
    public TMP_Text          slotQLabel;
    public TMP_Text          slotELabel;

    private SkillCardSO         _card;
    private Action<SkillCardSO> _onClicked;
    private ActiveSkillManager  _activeSkillManager;

    public void Setup(SkillCardSO card, Action<SkillCardSO> onClicked)
    {
        _card      = card;
        _onClicked = onClicked;
        _activeSkillManager = FindFirstObjectByType<ActiveSkillManager>();

        if (background   != null) background.color   = card.cardColor;
        if (iconImage    != null) iconImage.sprite   = card.icon;
        if (cardNameText != null) cardNameText.text  = card.cardName;
        if (rarityLabel  != null) rarityLabel.text   = RarityString(card.rarity);

        var handler    = FindFirstObjectByType<SkillCardHandler>();
        int stackCount = handler != null ? handler.GetStackCount(card) : 0;

        if (descriptionText != null)
            descriptionText.text = card.GetStackDescription(stackCount + 1);

        if (stackLabel != null)
        {
            stackLabel.gameObject.SetActive(stackCount > 0);
            stackLabel.text = $"Je hebt dit al {stackCount}×";
        }

        // Hide slot picker by default
        slotPickerPanel?.SetActive(false);
        cardContent?.SetActive(true);

        if (card is ActiveSkillCardSO)
            selectButton?.onClick.AddListener(OpenSlotPicker);
        else
            selectButton?.onClick.AddListener(() => _onClicked?.Invoke(_card));
    }

    public void SetInteractable(bool value)
    {
        if (selectButton  != null) selectButton.interactable  = value;
        if (slotQButton   != null) slotQButton.interactable   = value;
        if (slotEButton   != null) slotEButton.interactable   = value;
    }

    void OpenSlotPicker()
    {
        cardContent?.SetActive(false);
        slotPickerPanel?.SetActive(true);

        // Label slots with their key + occupied status
        RefreshSlotLabels();

        slotQButton?.onClick.RemoveAllListeners();
        slotEButton?.onClick.RemoveAllListeners();

        slotQButton?.onClick.AddListener(() => ConfirmSlot(KeyCode.Q));
        slotEButton?.onClick.AddListener(() => ConfirmSlot(KeyCode.E));
    }

    void RefreshSlotLabels()
    {
        if (_activeSkillManager == null) return;

        bool qOccupied = _activeSkillManager.IsSlotOccupied(KeyCode.Q);
        bool eOccupied = _activeSkillManager.IsSlotOccupied(KeyCode.E);

        if (slotQLabel != null)
            slotQLabel.text = qOccupied ? "Q  (bezet)" : "Q";

        if (slotELabel != null)
            slotELabel.text = eOccupied ? "E  (bezet)" : "E";

        // Disable occupied slot buttons so player can't double-assign
        if (slotQButton != null) slotQButton.interactable = !qOccupied;
        if (slotEButton != null) slotEButton.interactable = !eOccupied;
    }

    void ConfirmSlot(KeyCode key)
    {
        if (_card is ActiveSkillCardSO activeCard)
            _activeSkillManager.TryEquipActive(activeCard, key);

        _onClicked?.Invoke(_card);
    }

    static string RarityString(CardRarity r) => r switch
    {
        CardRarity.Common => "Gewoon",
        CardRarity.Rare   => "Zeldzaam",
        CardRarity.Epic   => "Episch",
        _                 => ""
    };
}