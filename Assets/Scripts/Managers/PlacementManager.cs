using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlacementManager : MonoBehaviour
{
    public Camera cam;
    public LayerMask placementLayer;
    public Collider playerZone;
    public LayerMask unitLayer;

    private GameObject _selectedPrefab;
    private bool _combatActive;

    private readonly Queue<GameObject> _queuedPrefabs = new Queue<GameObject>();

    private Unit _dragUnit;
    private Vector3 _dragOffset;
    private Vector3 _dragStartPos;
    private Quaternion _dragStartRot;

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
        _dragUnit = null;
    }

    public void StartCombat()
    {
        _combatActive = true;
        _selectedPrefab = null;
        CancelDrag();
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

        if (_dragUnit == null)
        {
            HandleStartDrag();
            HandlePlaceNewUnit();
        }
        else
        {
            HandleDragging();
        }
    }

    private void HandleStartDrag()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 500f, unitLayer)) return;

        Unit u = hit.collider.GetComponentInParent<Unit>();
        if (u == null) return;
        if (u.team != Team.Player) return;
        if (u.IsDead) return;

        _dragUnit = u;
        _dragStartPos = u.transform.position;
        _dragStartRot = u.transform.rotation;

        _dragOffset = u.transform.position - hit.point;

        var agent = _dragUnit.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private void HandleDragging()
    {
        if (_dragUnit == null) return;

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            _dragUnit.transform.position = _dragStartPos;
            _dragUnit.transform.rotation = _dragStartRot;
            CancelDrag();
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, placementLayer))
        {
            Vector3 pos = hit.point + _dragOffset;

            if (playerZone != null)
            {
                Vector3 closest = playerZone.ClosestPoint(pos);
                if ((closest - pos).sqrMagnitude > 0.0001f)
                {
                    pos = _dragUnit.transform.position;
                }
            }

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(pos, out navHit, 2f, NavMesh.AllAreas))
            {
                _dragUnit.transform.position = navHit.position;
            }
            else
            {
                _dragUnit.transform.position = pos;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var agent = _dragUnit.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(_dragUnit.transform.position, out navHit, 2f, NavMesh.AllAreas))
                {
                    agent.Warp(navHit.position);
                }
            }

            CancelDrag();
        }
    }

    private void CancelDrag()
    {
        _dragUnit = null;
    }

    private void HandlePlaceNewUnit()
    {
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

            GameObject go = Instantiate(_selectedPrefab, pos, Quaternion.identity);

            var agent = go.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(go.transform.position, out navHit, 2f, NavMesh.AllAreas))
                {
                    go.transform.position = navHit.position;
                    agent.Warp(navHit.position);
                }
            }

            _selectedPrefab = null;

            if (_queuedPrefabs.Count > 0)
                _selectedPrefab = _queuedPrefabs.Dequeue();
        }
    }
}