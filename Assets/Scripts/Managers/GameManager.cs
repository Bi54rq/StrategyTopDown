using TMPro;
using UnityEngine;

public enum GamePhase { Setup, Combat }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public EnemySpawner enemySpawner;
    public PlacementManager placementManager;
    public ShopManager shopManager;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI statusText;

    public int startingGold = 5;
    public int winGoldReward = 3;

    public int Gold { get; private set; }
    public int Round { get; private set; }
    public GamePhase Phase { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        Gold = startingGold;
        Round = 1;
        EnterSetup("Setup phase: buy + place units.");
    }

    private void Update()
    {
        UpdateUI();

        if (Phase == GamePhase.Combat)
        {
            int playerAlive = CountAlive(Team.Player);
            int enemyAlive = CountAlive(Team.Enemy);

            if (playerAlive == 0 || enemyAlive == 0)
            {
                if (enemyAlive == 0) OnWin();
                else OnLose();
            }
        }
    }

    public void PressBattle()
    {
        if (Phase != GamePhase.Setup) return;

        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;
            if (units[i].IsDead) continue;
            if (units[i].team != Team.Player) continue;
            units[i].RecordHome();
        }

        Phase = GamePhase.Combat;
        placementManager.StartCombat();
        SetStatus("Combat started.");
    }

    private void OnWin()
    {
        Gold += winGoldReward;
        Round += 1;
        EnterSetup($"Win! +{winGoldReward} gold. Round {Round}.");
    }

    private void OnLose()
    {
        EnterSetup("Defeat. Back to Start.");
    }

    private void EnterSetup(string message)
    {
        if (enemySpawner != null) enemySpawner.ClearEnemies();

        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;
            if (units[i].IsDead) continue;
            if (units[i].team != Team.Player) continue;
            units[i].ReturnHome();
        }

        Phase = GamePhase.Setup;
        placementManager.EndCombat();

        if (shopManager != null) shopManager.RerollFree();

        if (enemySpawner != null) enemySpawner.SpawnForRound(Round);

        SetStatus(message);
    }

    private int CountAlive(Team team)
    {
        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        int count = 0;

        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;
            if (units[i].IsDead) continue;
            if (units[i].team == team) count++;
        }

        return count;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (Gold < amount) return false;
        Gold -= amount;
        return true;
    }

    private void UpdateUI()
    {
        if (goldText != null) goldText.text = $"Gold: {Gold}";
        if (roundText != null) roundText.text = $"Round: {Round}";
    }

    public void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}