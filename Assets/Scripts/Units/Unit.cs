using System;
using System.Collections.Generic;
using UnityEngine;

public enum Team { Player, Enemy }
public enum UnitClass { Fighter, Tank, Ranged, Support }

public class Unit : MonoBehaviour
{
    public GameObject healthBarPrefab;

    public float tankTargetBiasChance = 0.65f;
    public int nearbyTargetSampleCount = 3;

    public Unit CurrentTarget => _target;

    private HealthBar _hb;
    private Vector3 _homePos;
    private Quaternion _homeRot;
    private bool _hasHome;

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

        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform);
            hb.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            hb.transform.localRotation = Quaternion.identity;

            _hb = hb.GetComponentInChildren<HealthBar>(true);

            if (_hb != null)
            {
                Camera c = Camera.main;
                if (c == null) c = FindFirstObjectByType<Camera>();
                _hb.Init(transform, c);
                _hb.Set01(CurrentHP / Mathf.Max(0.0001f, maxHP));
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.Phase != GamePhase.Combat) return;
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
        if (_hb != null) _hb.Set01(CurrentHP / Mathf.Max(0.0001f, maxHP));

        if (CurrentHP <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        if (amount <= 0f) return;

        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        if (_hb != null) _hb.Set01(CurrentHP / Mathf.Max(0.0001f, maxHP));
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (_hb != null) Destroy(_hb.gameObject);

        OnDied?.Invoke(this);
        Destroy(gameObject);
    }

    private Unit FindBestTarget()
    {
        Unit[] all = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        if (IsSupport())
        {
            Unit bestAlly = null;
            float bestAllyDist = float.MaxValue;

            for (int i = 0; i < all.Length; i++)
            {
                Unit other = all[i];
                if (other == null || other.IsDead || other == this) continue;
                if (other.team != this.team) continue;
                if (other.CurrentHP >= other.maxHP) continue;

                float d = Vector3.Distance(transform.position, other.transform.position);
                if (d < bestAllyDist)
                {
                    bestAllyDist = d;
                    bestAlly = other;
                }
            }

            return bestAlly;
        }

        List<Unit> enemies = new List<Unit>();

        for (int i = 0; i < all.Length; i++)
        {
            Unit other = all[i];
            if (other == null || other.IsDead || other == this) continue;
            if (other.team == this.team) continue;
            enemies.Add(other);
        }

        if (enemies.Count == 0) return null;

        enemies.Sort((a, b) =>
        {
            float da = Vector3.Distance(transform.position, a.transform.position);
            float db = Vector3.Distance(transform.position, b.transform.position);
            return da.CompareTo(db);
        });

        int sampleCount = Mathf.Min(nearbyTargetSampleCount, enemies.Count);

        List<Unit> nearbyTanks = new List<Unit>();
        for (int i = 0; i < sampleCount; i++)
        {
            if (enemies[i].unitClass == UnitClass.Tank)
                nearbyTanks.Add(enemies[i]);
        }

        if (nearbyTanks.Count > 0 && UnityEngine.Random.value < tankTargetBiasChance)
        {
            Unit nearestTank = nearbyTanks[0];
            float bestTankDist = Vector3.Distance(transform.position, nearestTank.transform.position);

            for (int i = 1; i < nearbyTanks.Count; i++)
            {
                float d = Vector3.Distance(transform.position, nearbyTanks[i].transform.position);
                if (d < bestTankDist)
                {
                    bestTankDist = d;
                    nearestTank = nearbyTanks[i];
                }
            }

            return nearestTank;
        }

        return enemies[0];
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

    public void RecordHome()
    {
        _homePos = transform.position;
        _homeRot = transform.rotation;
        _hasHome = true;
    }

    public void ReturnHome()
    {
        if (!_hasHome) return;

        transform.position = _homePos;
        transform.rotation = _homeRot;

        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }
}