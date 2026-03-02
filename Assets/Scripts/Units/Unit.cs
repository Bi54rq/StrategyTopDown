using System;
using UnityEngine;

public enum Team { Player, Enemy }
public enum UnitClass { Fighter, Tank, Ranged, Support }

public class Unit : MonoBehaviour
{
    public Team team = Team.Player;
    public UnitClass unitClass = UnitClass.Fighter;

    public float maxHP = 10f;
    public float damage = 2f;
    public float healing = 2f;

    public float moveSpeed = 3f;
    public float attackRange = 1.2f;
    public float attacksPerSecond = 1f;

    public float CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    private Unit _target;
    private float _cooldown;

    public event Action<Unit> OnDied;

    private void Awake()
    {
        CurrentHP = maxHP;
        _cooldown = 0f;
    }

    private void Update()
    {
        if (IsDead) return;

        if (_cooldown > 0f)
            _cooldown -= Time.deltaTime;

        if (_target == null || _target.IsDead || !IsTargetStillValid(_target))
            _target = FindBestTarget();

        if (_target == null) return;

        float dist = Vector3.Distance(transform.position, _target.transform.position);

        if (dist > attackRange)
        {
            Vector3 dir = (_target.transform.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
        else
        {
            if (_cooldown <= 0f)
            {
                PerformActionOnTarget(_target);
                float rate = Mathf.Max(0.05f, attacksPerSecond);
                _cooldown = 1f / rate;
            }
        }
    }

    private bool IsSupport()
    {
        return unitClass == UnitClass.Support;
    }

    private void PerformActionOnTarget(Unit target)
    {
        if (IsSupport())
            target.Heal(healing);
        else
            target.TakeDamage(damage);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        CurrentHP -= amount;
        if (CurrentHP <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;
        OnDied?.Invoke(this);
        Destroy(gameObject);
    }

    private Unit FindBestTarget()
    {
        Unit[] all = FindObjectsOfType<Unit>();

        Unit best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < all.Length; i++)
        {
            Unit other = all[i];
            if (other == null || other.IsDead || other == this) continue;

            if (IsSupport())
            {
                if (other.team != this.team) continue;
                if (other.CurrentHP >= other.maxHP) continue;
            }
            else
            {
                if (other.team == this.team) continue;
            }

            float d = Vector3.Distance(transform.position, other.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = other;
            }
        }

        return best;
    }

    private bool IsTargetStillValid(Unit target)
    {
        if (target == null || target.IsDead) return false;

        if (IsSupport())
        {
            if (target.team != this.team) return false;
            if (target.CurrentHP >= target.maxHP) return false;
            return true;
        }

        return target.team != this.team;
    }
}