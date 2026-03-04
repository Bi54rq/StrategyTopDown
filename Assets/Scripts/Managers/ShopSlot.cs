using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI label;
    public GameObject soldOverlay;

    public bool IsSold { get; private set; }
    public ShopOffer Offer { get; private set; }

    private ShopManager _shop;

    public void SetOffer(ShopOffer offer, ShopManager shop)
    {
        Offer = offer;
        _shop = shop;
        IsSold = false;

        if (soldOverlay != null) soldOverlay.SetActive(false);
        if (label != null) label.text = $"{offer.displayName}\nCost {offer.cost}";

        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    public void MarkSold()
    {
        IsSold = true;

        if (soldOverlay != null) soldOverlay.SetActive(true);
        if (label != null) label.text = "SOLD";

        if (button != null) button.interactable = false;
    }

    private void OnClick()
    {
        if (_shop == null) return;
        _shop.BuyFromSlot(this);
    }
}