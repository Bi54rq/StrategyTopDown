using System.Collections.Generic;
using UnityEngine;

public class EnemyPreviewSpawner : MonoBehaviour
{
    public Collider enemyZone;
    public GameObject fighterPrefab;
    public GameObject tankPrefab;
    public GameObject rangedPrefab;
    public GameObject supportPrefab;

    public int minEnemies = 2;
    public int maxEnemies = 4;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    public void ClearEnemies()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
        }
        _spawned.Clear();
    }

    public void SpawnPreviewForRound(int round, int seedOffset = 0)
    {
        ClearEnemies();
        if (enemyZone == null) return;

        int seed = (round * 1000) + seedOffset;
        var rng = new System.Random(seed);

        int count = Mathf.Clamp(rng.Next(minEnemies, maxEnemies + 1), 1, 99);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = RollPrefab(rng);
            if (prefab == null) continue;

            Vector3 pos = RandomPointInZone(enemyZone, rng);
            GameObject go = Instantiate(prefab, pos, Quaternion.identity);

            Unit u = go.GetComponent<Unit>();
            if (u != null) u.team = Team.Enemy;

            _spawned.Add(go);
        }
    }

    private GameObject RollPrefab(System.Random rng)
    {
        int r = rng.Next(0, 100);
        if (r < 40) return fighterPrefab;
        if (r < 65) return tankPrefab;
        if (r < 90) return rangedPrefab;
        return supportPrefab;
    }

    private Vector3 RandomPointInZone(Collider zone, System.Random rng)
    {
        Bounds b = zone.bounds;

        for (int tries = 0; tries < 30; tries++)
        {
            float x = Mathf.Lerp(b.min.x, b.max.x, (float)rng.NextDouble());
            float z = Mathf.Lerp(b.min.z, b.max.z, (float)rng.NextDouble());
            Vector3 p = new Vector3(x, b.center.y, z);

            Vector3 closest = zone.ClosestPoint(p);
            if ((closest - p).sqrMagnitude < 0.0001f)
                return p;
        }

        return b.center;
    }
}