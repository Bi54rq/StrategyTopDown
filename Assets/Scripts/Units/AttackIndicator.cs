using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AttackIndicator : MonoBehaviour
{
    public Unit unit;
    public float heightOffset = 0.4f;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();

        if (unit == null)
            unit = GetComponent<Unit>();

        line.positionCount = 2;
        line.enabled = false;
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Combat)
        {
            line.enabled = false;
            return;
        }

        if (unit == null || unit.CurrentTarget == null || unit.CurrentTarget.IsDead)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;

        Vector3 start = unit.transform.position + Vector3.up * heightOffset;
        Vector3 end = unit.CurrentTarget.transform.position + Vector3.up * heightOffset;

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        if (unit.unitClass == UnitClass.Support)
        {
            line.startColor = Color.green;
            line.endColor = Color.green;
        }
        else if (unit.team == Team.Player)
        {
            line.startColor = Color.blue;
            line.endColor = Color.blue;
        }
        else
        {
            line.startColor = Color.red;
            line.endColor = Color.red;
        }
    }
}