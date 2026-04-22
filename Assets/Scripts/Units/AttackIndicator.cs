using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AttackIndicator : MonoBehaviour
{
    public Unit unit;
    public float heightOffset = 0.4f;
    public float lineSeparation = 0.2f;

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

        if (unit == null)
        {
            line.enabled = false;
            return;
        }

        if (unit.CurrentTarget == null || unit.CurrentTarget.IsDead)
        {
            line.enabled = false;
            return;
        }

        line.enabled = true;

        Vector3 start = unit.transform.position + Vector3.up * heightOffset;
        Vector3 end = unit.CurrentTarget.transform.position + Vector3.up * heightOffset;

        bool isMutualTarget =
    unit.CurrentTarget.CurrentTarget != null &&
    unit.CurrentTarget.CurrentTarget == unit;

        if (isMutualTarget && unit.unitClass != UnitClass.Support)
        {
            Vector3 screenSide = Vector3.right;

            if (Camera.main != null)
            {
                screenSide = Camera.main.transform.right;
                screenSide.y = 0f;
                screenSide.Normalize();
            }

            if (unit.team == Team.Player)
            {
                start -= screenSide * lineSeparation;
                end -= screenSide * lineSeparation;
            }
            else
            {
                start += screenSide * lineSeparation;
                end += screenSide * lineSeparation;
            }
        }

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        if (unit.unitClass == UnitClass.Support)
        {
            Color healColor = new Color(0.2f, 1f, 0.4f, 0.85f);
            line.startColor = healColor;
            line.endColor = healColor;
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
        }
        else if (unit.team == Team.Player)
        {
            Color playerColor = new Color(0.2f, 0.5f, 1f, 0.85f);
            line.startColor = playerColor;
            line.endColor = playerColor;
            line.startWidth = 0.04f;
            line.endWidth = 0.04f;
        }
        else
        {
            Color enemyColor = new Color(1f, 0.2f, 0.2f, 0.85f);
            line.startColor = enemyColor;
            line.endColor = enemyColor;
            line.startWidth = 0.04f;
            line.endWidth = 0.04f;
        }
    }
}