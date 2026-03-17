using System.Collections.Generic;
using UnityEngine;

public class EnemyPreviewSpawner : MonoBehaviour
{
    public Transform[] previewSpawnPoints;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    public void ClearEnemies()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
        }
        _spawned.Clear();
    }

    public void SpawnPreviewFromPlan(List<GameObject> plannedPrefabs, int round, int seedOffset = 0)
    {
        ClearEnemies();

        if (plannedPrefabs == null || plannedPrefabs.Count == 0) return;
        if (previewSpawnPoints == null || previewSpawnPoints.Length == 0) return;

        int spCount = previewSpawnPoints.Length;

        for (int i = 0; i < plannedPrefabs.Count; i++)
        {
            GameObject prefab = plannedPrefabs[i];
            if (prefab == null) continue;

            Transform sp = previewSpawnPoints[i % spCount];
            GameObject go = Instantiate(prefab, sp.position, sp.rotation);

            MakePreviewOnly(go);

            _spawned.Add(go);
        }
    }

    private void MakePreviewOnly(GameObject go)
    {
        Unit unit = go.GetComponent<Unit>();
        if (unit != null)
            Destroy(unit);

        AttackIndicator attackIndicator = go.GetComponent<AttackIndicator>();
        if (attackIndicator != null)
            Destroy(attackIndicator);

        var agent = go.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
            Destroy(agent);

        Collider[] colliders = go.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody[] rigidbodies = go.GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
        }
    }
}