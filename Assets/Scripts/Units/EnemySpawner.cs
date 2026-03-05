using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] enemyPrefabs;

    public int baseEnemyCount = 2;
    public int maxEnemyCount = 8;

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

    public void SpawnForRound(int round)
    {
        ClearEnemies();

        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        int count = Mathf.Clamp(baseEnemyCount + (round - 1), 1, maxEnemyCount);
        int spCount = spawnPoints.Length;

        for (int i = 0; i < count; i++)
        {
            Transform sp = spawnPoints[i % spCount];
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            GameObject go = Instantiate(prefab, sp.position, Quaternion.identity);

            Unit u = go.GetComponent<Unit>();
            if (u != null) u.team = Team.Enemy;
        }
    }
}