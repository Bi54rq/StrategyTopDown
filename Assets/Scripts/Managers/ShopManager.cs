using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ShopOffer
{
    public string displayName;
    public GameObject prefab;
    public int cost;
}

public class ShopManager : MonoBehaviour
{
    public PlacementManager placementManager;
    public GameManager gameManager;

    public ShopSlot[] slots;
    public ShopOffer[] offerPool;

    public int rerollCost = 1;

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

        for (int i = 0; i < slots.Length; i++)
        {
            int idx = UnityEngine.Random.Range(0, offerPool.Length);
            slots[i].SetOffer(offerPool[idx], this);
        }

        gameManager.SetStatus(paid ? $"Rerolled (-{rerollCost} gold)." : "New round shop rerolled.");
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
        gameManager.SetStatus($"Bought {slot.Offer.displayName} (-{cost} gold).");
    }
}