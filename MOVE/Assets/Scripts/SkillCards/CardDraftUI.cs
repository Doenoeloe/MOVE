using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardDraftUI : MonoBehaviour
{
    [Header("References")]
    public Transform   cardContainer;
    public GameObject  cardOfferPrefab;
    public TMP_Text    levelUpLabel;
    public CanvasGroup canvasGroup;

    private Action<SkillCardSO> _onPicked;
    private List<CardOfferUI>   _spawnedOffers = new();

    public void ShowDraft(List<SkillCardSO> offers, Action<SkillCardSO> onPicked)
    {
        _onPicked = onPicked;
        canvasGroup.alpha = 1f;

        if (levelUpLabel != null)
            levelUpLabel.text = "Level up — kies een upgrade";

        SpawnOffers(offers);

        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger("Show");
        }

        Time.timeScale = 0f;
    }

    // Called by CardDraftController after all queued drafts are done
    public void HidePanel()
    {
        var anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Hide");

        StartCoroutine(HideThenDeactivate());
    }

    IEnumerator HideThenDeactivate()
    {
        yield return new WaitForSecondsRealtime(0.35f);
        canvasGroup.alpha = 0f;
        Time.timeScale = 1f;
    }

    void SpawnOffers(List<SkillCardSO> offers)
    {
        foreach (var old in _spawnedOffers)
            if (old != null) Destroy(old.gameObject);
        _spawnedOffers.Clear();

        if (cardOfferPrefab == null)
        {
            Debug.LogError("[CardDraftUI] cardOfferPrefab is niet ingevuld!");
            return;
        }

        if (cardContainer == null)
        {
            Debug.LogError("[CardDraftUI] cardContainer is niet ingevuld!");
            return;
        }

        foreach (var card in offers)
        {
            var go      = Instantiate(cardOfferPrefab, cardContainer);
            var offerUI = go.GetComponent<CardOfferUI>();

            if (offerUI == null)
            {
                Debug.LogError("[CardDraftUI] cardOfferPrefab heeft geen CardOfferUI component!");
                continue;
            }

            offerUI.Setup(card, OnOfferClicked);
            _spawnedOffers.Add(offerUI);
        }
    }

    void OnOfferClicked(SkillCardSO card)
    {
        foreach (var offer in _spawnedOffers)
            offer.SetInteractable(false);

        // Don't hide yet — let the controller decide if another draft follows
        _onPicked?.Invoke(card);
    }
}