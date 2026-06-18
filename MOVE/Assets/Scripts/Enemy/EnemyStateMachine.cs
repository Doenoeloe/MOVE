using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Centrale AI state machine voor alle enemy types.
/// Hergebruikt op: MeleeEnemy, RangedEnemy, ShieldEnemy, BossEnemy
///
/// Optionele componenten op hetzelfde GameObject bepalen het gedrag:
///   - RangedAttackBehaviour  → vuurt projectiel in plaats van melee hit
///   - ArmorComponent         → filtert schade voor HealthComponent
///   - ParrySignal            → toont telegraph glow (melee enemies)
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(FactionComponent))]
public class EnemyStateMachine : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data;

    public enum AIState { Idle, Approach, Telegraph, Attacking, Stagger, Recover, Dead }
    public AIState CurrentState { get; private set; } = AIState.Idle;

    public bool IsTargetable => CurrentState != AIState.Stagger
                             && CurrentState != AIState.Dead;
    public bool IsHittable   => CurrentState != AIState.Dead;

    public event Action<AIState> OnStateChanged;

    // Core — altijd aanwezig
    private NavMeshAgent    _agent;
    private HealthComponent _health;
    private EnemyAI         _enemyAI;
    private CombatArena     _arena;
    private Transform       _player;
    private EnemyVisuals    _visuals;
    private XPSystem        _xpSystem;
    private SkillCardHandler _handler;
    private float           _stateTimer;
    private bool            _xpAwarded;

    // Optionele componenten — aanwezig afhankelijk van enemy type
    private RangedAttackBehaviour _ranged;   // RangedEnemy, BossEnemy
    private ArmorComponent        _armor;    // ShieldEnemy, BossEnemy
    private ParrySignal           _parry;    // MeleeEnemy, ShieldEnemy, BossEnemy

    void Awake()
    {
        _agent   = GetComponent<NavMeshAgent>();
        _health  = GetComponent<HealthComponent>();
        _visuals = GetComponent<EnemyVisuals>();
        _arena   = CombatArena.Instance;
        _enemyAI = GetComponent<EnemyAI>();
        _xpSystem = FindAnyObjectByType<XPSystem>();
        _handler  = FindAnyObjectByType<SkillCardHandler>();

        // Optionele componenten — null als ze er niet op zitten
        _ranged = GetComponent<RangedAttackBehaviour>();
        _armor  = GetComponent<ArmorComponent>();
        _parry  = GetComponent<ParrySignal>();

        if (data != null && _agent != null)
            _agent.speed = data.moveSpeed;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;

        if (data == null)
            Debug.LogError($"[EnemyStateMachine] {name} heeft geen EnemyData asset!");
    }

    void OnEnable()
    {
        _health.OnDeath += _ => EnterState(AIState.Dead);

        _health.OnHealthChanged += (hp, _) =>
        {
            if (CurrentState == AIState.Dead) return;
            if (CurrentState == AIState.Telegraph || CurrentState == AIState.Attacking)
                _arena?.ReleaseAttackSlot(_enemyAI);
            EnterState(AIState.Stagger);
        };
    }
    
    void Start()
    {
        if (_arena == null)
            _arena = CombatArena.Instance;
        
        _arena?.RegisterEnemy(_enemyAI);
    }
    
    void Update()
    {
        if (CurrentState == AIState.Dead)
        {
            if (!_xpAwarded)
            {
                _xpAwarded = true;
                _xpSystem?.AddXP(data.xpReward);
                _handler?.NotifyKill();
            }
            return;
        }

        _stateTimer -= Time.deltaTime;

        switch (CurrentState)
        {
            case AIState.Idle:      UpdateIdle();      break;
            case AIState.Approach:  UpdateApproach();  break;
            case AIState.Telegraph: UpdateTelegraph(); break;
            case AIState.Attacking: UpdateAttacking(); break;
            case AIState.Stagger:   UpdateStagger();   break;
            case AIState.Recover:   UpdateRecover();   break;
        }

        _visuals.SetForState(CurrentState);
    }

    void UpdateIdle()
    {
        if (_player != null && DistToPlayer() < data.aggroRange)
            EnterState(AIState.Approach);
    }

    void UpdateApproach()
    {
        if (_player == null) return;
        _agent.SetDestination(_player.position);

        if (DistToPlayer() <= data.attackRange)
        {
            _agent.ResetPath();
            if (_arena != null && _arena.RequestAttack(_enemyAI))
                EnterState(AIState.Telegraph);
        }
    }

    void UpdateTelegraph()
    {
        FacePlayer();
        if (_stateTimer <= 0f) EnterState(AIState.Attacking);
    }

    void UpdateAttacking()
    {
        FacePlayer();

        if (_stateTimer <= 0f)
        {
            if (_ranged != null)
            {
                // Ranged enemy: vuur projectiel
                _ranged.FireAtPlayer();
            }
            else
            {
                // Melee enemy: directe hit op speler
                var switcher = _player?.GetComponent<CharacterSwitchManager>();
                var activeGO = switcher?.GetActiveCharacter();
                activeGO?.GetComponent<IHittable>()?.OnEnemyAttackLanded(transform);
            }

            _arena?.ReleaseAttackSlot(_enemyAI);
            EnterState(AIState.Recover);
        }
    }

    void UpdateStagger()
    {
        if (_stateTimer <= 0f) EnterState(AIState.Recover);
    }

    void UpdateRecover()
    {
        if (_stateTimer <= 0f)
            EnterState(DistToPlayer() < data.aggroRange ? AIState.Approach : AIState.Idle);
    }

    public void EnterState(AIState next)
    {
        if (CurrentState == AIState.Dead && next != AIState.Dead) return;

        CurrentState = next;
        OnStateChanged?.Invoke(next);
        _visuals.SetForState(next);

        switch (next)
        {
            case AIState.Idle:
            case AIState.Approach:
                MoveAgent(false);
                break;

            case AIState.Telegraph:
                _stateTimer = data.telegraphDuration;
                MoveAgent(true);
                _parry?.TriggerGlow(data.telegraphDuration);
                break;

            case AIState.Attacking:
                _stateTimer = data.attackDuration;
                break;

            case AIState.Stagger:
                _stateTimer = data.staggerDuration;
                MoveAgent(true);
                _parry?.CancelGlow();
                break;

            case AIState.Recover:
                _stateTimer = data.recoverDuration;
                MoveAgent(true);
                break;

            case AIState.Dead:
                if (_agent.enabled && _agent.isOnNavMesh) _agent.ResetPath();
                _agent.enabled = false;
                break;
        }
    }

    /// <summary>
    /// Filtert schade via ArmorComponent als die aanwezig is.
    /// Aangeroepen door EnemyAI.TakeDamage.
    /// </summary>
    public float FilterDamage(float raw)
        => _armor != null ? _armor.FilterDamage(raw) : raw;

    public void OnCountered()
    {
        _arena?.ReleaseAttackSlot(_enemyAI);
        EnterState(AIState.Stagger);
    }

    public void DEBUG_ForceAttack()
    {
        if (CurrentState != AIState.Approach && CurrentState != AIState.Idle) return;
        _agent.ResetPath();
        if (_arena != null && _arena.RequestAttack(_enemyAI))
            EnterState(AIState.Telegraph);
    }

    void MoveAgent(bool stopped)
    {
        if (_agent.enabled && _agent.isOnNavMesh)
            _agent.isStopped = stopped;
    }

    void FacePlayer()
    {
        if (_player == null) return;
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
    }

    float DistToPlayer() =>
        _player == null ? float.MaxValue
                        : Vector3.Distance(transform.position, _player.position);

    void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}