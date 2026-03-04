using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Camera cam;
    public LayerMask placementLayer;
    public Collider playerZone;

    private GameObject _selectedPrefab;
    private bool _combatActive;

    private readonly Queue<GameObject> _queuedPrefabs = new Queue<GameObject>();

    public bool CombatActive => _combatActive;

    public void SelectToPlace(GameObject prefab)
    {
        if (prefab == null) return;

        if (_combatActive)
        {
            _queuedPrefabs.Enqueue(prefab);
            return;
        }

        _selectedPrefab = prefab;
    }

    public void StartCombat()
    {
        _combatActive = true;
        _selectedPrefab = null;
    }

    public void EndCombat()
    {
        _combatActive = false;

        if (_selectedPrefab == null && _queuedPrefabs.Count > 0)
            _selectedPrefab = _queuedPrefabs.Dequeue();
    }

    public void CancelPlacement()
    {
        _selectedPrefab = null;
    }

    private void Update()
    {
        if (_combatActive) return;
        if (_selectedPrefab == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, placementLayer)) return;

            Vector3 pos = hit.point;

            if (playerZone != null)
            {
                Vector3 closest = playerZone.ClosestPoint(pos);
                if ((closest - pos).sqrMagnitude > 0.0001f)
                    return;
            }

            Instantiate(_selectedPrefab, pos, Quaternion.identity);
            _selectedPrefab = null;

            if (_queuedPrefabs.Count > 0)
                _selectedPrefab = _queuedPrefabs.Dequeue();
        }
    }
}