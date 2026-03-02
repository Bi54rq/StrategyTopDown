using UnityEngine;
using UnityEngine.UI;

public class BattleButtonUI : MonoBehaviour
{
    public PlacementManager placementManager;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Update()
    {
        if (placementManager == null || _button == null) return;
        _button.interactable = !placementManager.CombatActive;
    }
}