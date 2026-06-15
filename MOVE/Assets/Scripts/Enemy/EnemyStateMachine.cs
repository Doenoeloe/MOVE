using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(FactionComponent))]
public class EnemyStateMachine : MonoBehaviour
{
    [Header("Data")]
    public EnemyData data; // Sleep een EnemyData asset hierheen in de Inspector
 
    public enum AIState { Idle, Approach, Telegraph, Attacking, Stagger, Recover, Dead }
    public AIState CurrentState { get; private set; } = AIState.Idle;
 
    public bool IsTargetable => CurrentState != AIState.Stagger
                             && CurrentState != AIState.Dead;
    public bool IsHittable   => CurrentState != AIState.Dead;
 
    public event Action<AIState> OnStateChanged;
 
    private NavMeshAgent    _agent;
    private HealthComponent _health;
    private EnemyAI         _enemyAI;  // cached — avoids GetComponent every frame
    private CombatArena     _arena;
    private Transform       _player;
    private EnemyVisuals    _visuals;
    private float           _stateTimer;
 
    void Awake()
    {
        _agent   = GetComponent<NavMeshAgent>();
        _health  = GetComponent<HealthComponent>();
        _visuals = GetComponent<EnemyVisuals>();
        _arena   = CombatArena.Instance;
        _enemyAI = GetComponent<EnemyAI>();
 
        // Gebruik data waarden voor NavMeshAgent snelheid
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
            if (CurrentState != AIState.Dead)
                EnterState(AIState.Stagger);
        };
    }
 
    void Update()
    {
        if (CurrentState == AIState.Dead) return;
        _stateTimer -= Time.deltaTime;
 
        switch (CurrentState)
        {
            case AIState.Idle:       UpdateIdle(); _visuals.SetForState(AIState.Idle);      break;
            case AIState.Approach:   UpdateApproach(); _visuals.SetForState(AIState.Approach);   break;
            case AIState.Telegraph:  UpdateTelegraph(); _visuals.SetForState(AIState.Telegraph);  break;
            case AIState.Attacking:  UpdateAttacking(); _visuals.SetForState(AIState.Attacking);  break;
            case AIState.Stagger:    UpdateStagger();  _visuals.SetForState(AIState.Stagger);  break;
            case AIState.Recover:    UpdateRecover(); _visuals.SetForState(AIState.Recover);   break;
        }
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
        if (CurrentState != AIState.Attacking) return;
        FacePlayer();
 
        if (_stateTimer <= 0f)
        {
            var switcher = _player?.GetComponent<CharacterSwitchManager>();
            var activeGO = switcher?.GetActiveCharacter();
            activeGO?.GetComponent<IHittable>()?.OnEnemyAttackLanded(transform);
 
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
        _visuals.SetForState(next);
        CurrentState = next;
        OnStateChanged?.Invoke(next);
 
        switch (next)
        {
            case AIState.Idle:
            case AIState.Approach:
                MoveAgent(false);
                break;
            case AIState.Telegraph:
                _stateTimer = data.telegraphDuration;
                MoveAgent(true);
                GetComponent<ParrySignal>()?.TriggerGlow(data.telegraphDuration);
                break;
            case AIState.Attacking:
                _stateTimer = data.attackDuration;
                break;
            case AIState.Stagger:
                _stateTimer = data.staggerDuration;
                MoveAgent(true);
                GetComponent<ParrySignal>()?.CancelGlow();
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
