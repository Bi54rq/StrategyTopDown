using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;

    public int baseEnemyCount = 2;
    public int maxEnemyCount = 8;

    [Header("Per Class Limits (0 = Unlimited)")]
    public int maxFightersPerRound = 0;
    public int maxTanksPerRound = 0;
    public int maxRangedPerRound = 0;
    public int maxSupportsPerRound = 0;

    private readonly List<GameObject> _preparedTeam = new List<GameObject>();

    public void ClearEnemies()
    {
        var units = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            if (units[i] == null) continue;
            if (units[i].team != Team.Enemy) continue;
            Destroy(units[i].gameObject);
        }
    }

    public void PrepareRound(int round)
    {
        _preparedTeam.Clear();

        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        int count = Mathf.Clamp(baseEnemyCount + (round - 1), 1, maxEnemyCount);

        Dictionary<UnitClass, int> classCounts = new Dictionary<UnitClass, int>();

        for (int i = 0; i < count; i++)
        {
            List<GameObject> validChoices = new List<GameObject>();

            for (int j = 0; j < enemyPrefabs.Length; j++)
            {
                GameObject prefab = enemyPrefabs[j];
                if (prefab == null) continue;

                Unit unit = prefab.GetComponent<Unit>();
                if (unit == null) continue;

                int currentCount = GetCurrentCount(classCounts, unit.unitClass);
                int maxAllowed = GetMaxAllowedForClass(unit.unitClass);

                bool unlimited = maxAllowed == 0;
                bool underLimit = currentCount < maxAllowed;

                if (unlimited || underLimit)
                    validChoices.Add(prefab);
            }

            if (validChoices.Count == 0)
            {
                Debug.LogWarning("EnemySpawner: No more valid enemy prefabs available under the class limits.");
                break;
            }

            GameObject chosenPrefab = validChoices[Random.Range(0, validChoices.Count)];
            _preparedTeam.Add(chosenPrefab);

            Unit chosenUnit = chosenPrefab.GetComponent<Unit>();
            if (chosenUnit != null)
            {
                if (!classCounts.ContainsKey(chosenUnit.unitClass))
                    classCounts[chosenUnit.unitClass] = 0;

                classCounts[chosenUnit.unitClass]++;
            }
        }
    }

    public void SpawnPreparedRound()
    {
        ClearEnemies();

        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (_preparedTeam.Count == 0) return;

        int spCount = spawnPoints.Length;

        for (int i = 0; i < _preparedTeam.Count; i++)
        {
            GameObject prefab = _preparedTeam[i];
            if (prefab == null) continue;

            Transform sp = spawnPoints[i % spCount];
            GameObject go = Instantiate(prefab, sp.position, Quaternion.identity);

            Unit u = go.GetComponent<Unit>();
            if (u != null) u.team = Team.Enemy;
        }
    }

    public List<GameObject> GetPreparedTeam()
    {
        return new List<GameObject>(_preparedTeam);
    }

    public bool HasPreparedTeam()
    {
        return _preparedTeam.Count > 0;
    }

    private int GetCurrentCount(Dictionary<UnitClass, int> classCounts, UnitClass unitClass)
    {
        int count = 0;
        classCounts.TryGetValue(unitClass, out count);
        return count;
    }

    private int GetMaxAllowedForClass(UnitClass unitClass)
    {
        switch (unitClass)
        {
            case UnitClass.Fighter:
                return maxFightersPerRound;

            case UnitClass.Tank:
                return maxTanksPerRound;

            case UnitClass.Ranged:
                return maxRangedPerRound;

            case UnitClass.Support:
                return maxSupportsPerRound;

            default:
                return 0;
        }
    }
}