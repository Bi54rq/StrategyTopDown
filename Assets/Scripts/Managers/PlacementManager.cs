using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public Camera cam;
    public LayerMask placementLayer;
    public Collider playerZone;

    public GameObject fighterPrefab;
    public GameObject tankPrefab;
    public GameObject rangedPrefab;
    public GameObject supportPrefab;

    private GameObject _selectedPrefab;
    private bool _combatActive;

    public bool CombatActive => _combatActive;

    public void SelectFighter() => _selectedPrefab = fighterPrefab;
    public void SelectTank() => _selectedPrefab = tankPrefab;
    public void SelectRanged() => _selectedPrefab = rangedPrefab;
    public void SelectSupport() => _selectedPrefab = supportPrefab;

    public void StartCombat()
    {
        _combatActive = true;
        _selectedPrefab = null;
    }

    public void EndCombat()
    {
        _combatActive = false;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
            EndCombat();

        if (_combatActive) return;
        if (_selectedPrefab == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 500f, placementLayer)) return;

            Vector3 pos = hit.point;

            if (playerZone != null && !playerZone.bounds.Contains(pos))
                return;

            Instantiate(_selectedPrefab, pos, Quaternion.identity);
            _selectedPrefab = null;
        }
    }
}