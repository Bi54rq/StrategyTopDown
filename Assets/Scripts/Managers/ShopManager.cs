using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopOffer
{
    public string displayName;
    public GameObject prefab;
    public int cost;
    public Color displayColor = Color.white;
}

public class ShopManager : MonoBehaviour
{
    public PlacementManager placementManager;
    public GameManager gameManager;

    public ShopSlot[] slots;
    public ShopOffer[] offerPool;

    public int rerollCost = 1;
    public int maxSameClassPerShop = 2;

    public void RerollFree()
    {
        RerollInternal(false);
    }

    public void RerollPaid()
    {
        RerollInternal(true);
    }

    private void RerollInternal(bool paid)
    {
        if (offerPool == null || offerPool.Length == 0) return;
        if (slots == null || slots.Length == 0) return;

        if (paid)
        {
            if (!gameManager.TrySpendGold(rerollCost))
            {
                gameManager.SetStatus("Not enough gold to reroll.");
                return;
            }
        }

        Dictionary<UnitClass, int> classCounts = new Dictionary<UnitClass, int>();

        for (int i = 0; i < slots.Length; i++)
        {
            List<ShopOffer> validOffers = new List<ShopOffer>();

            for (int j = 0; j < offerPool.Length; j++)
            {
                ShopOffer offer = offerPool[j];
                if (offer == null || offer.prefab == null) continue;

                Unit unit = offer.prefab.GetComponent<Unit>();
                if (unit == null) continue;

                int currentCount = 0;
                classCounts.TryGetValue(unit.unitClass, out currentCount);

                if (currentCount < maxSameClassPerShop)
                    validOffers.Add(offer);
            }

            if (validOffers.Count == 0)
            {
                slots[i].SetOffer(null, this);
                continue;
            }

            ShopOffer chosenOffer = validOffers[UnityEngine.Random.Range(0, validOffers.Count)];
            slots[i].SetOffer(chosenOffer, this);

            Unit chosenUnit = chosenOffer.prefab.GetComponent<Unit>();
            if (chosenUnit != null)
            {
                if (!classCounts.ContainsKey(chosenUnit.unitClass))
                    classCounts[chosenUnit.unitClass] = 0;

                classCounts[chosenUnit.unitClass]++;
            }
        }

        gameManager.SetStatus(paid ? $"Rerolled (-{rerollCost} gold)." : "New round shop reroll.");
    }

    public void BuyFromSlot(ShopSlot slot)
    {
        if (slot == null) return;
        if (slot.IsSold) return;
        if (slot.Offer == null || slot.Offer.prefab == null) return;

        int cost = slot.Offer.cost;

        if (!gameManager.TrySpendGold(cost))
        {
            gameManager.SetStatus("Not enough gold.");
            return;
        }

        placementManager.SelectToPlace(slot.Offer.prefab);
        slot.MarkSold();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBuy();

        gameManager.SetStatus($"Bought {slot.Offer.displayName} (-{cost} gold).");
    }
}