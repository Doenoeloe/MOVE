using UnityEngine;
using UnityEngine.AI;

/// Full test-dummy enemy with a simple state machine:
///   Idle → Approach → Telegraph → Attack → Stagger → Recover → Idle
///
/// Requires: NavMeshAgent, CombatArena in scene.
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────

    [Header("Detection")]
    public float aggroRange    = 10f;
    public float attackRange   = 1.8f;

    [Header("Timing")]
    public float telegraphDuration = 0.6f;
    public float attackDuration    = 0.4f;
    public float recoverDuration   = 1.0f;
    public float staggerDuration   = 1.2f;

    [Header("Debug Colors")]
    public Color telegraphColor = new Color(1f, 0.6f, 0f, 1f);
    public Color attackColor    = new Color(1f, 0.1f, 0.1f, 1f);
    public Color staggerColor   = new Color(0.4f, 0.4f, 1f, 1f);

    [Header("Health")]
    public float maxHealth = 100f;
    public float Health    { get; private set; }

    // ── State ──────────────────────────────────────────────────

    public enum AIState { Idle, Approach, Telegraph, Attacking, Stagger, Recover, Dead }
    public AIState CurrentState  { get; private set; } = AIState.Idle;
    public bool    IsTargetable  => CurrentState != AIState.Stagger
                                 && CurrentState != AIState.Dead;
    public bool IsHittable    => CurrentState != AIState.Dead;
    // ── Private ────────────────────────────────────────────────

    private NavMeshAgent    _agent;
    private CombatArena     _arena;
    private Transform       _player;
    private float           _stateTimer;
    private Renderer        _renderer;
    private Color           _baseColor;

    // ── Unity ──────────────────────────────────────────────────

    void Awake()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _renderer = GetComponentInChildren<Renderer>();
        _arena    = FindObjectOfType<CombatArena>();

        if (_renderer != null)
            _baseColor = _renderer.material.color;

        Health = maxHealth;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    void Update()
    {
        _stateTimer -= Time.deltaTime;

        switch (CurrentState)
        {
            case AIState.Idle:       UpdateIdle();      break;
            case AIState.Approach:   UpdateApproach();  break;
            case AIState.Telegraph:  UpdateTelegraph(); break;
            case AIState.Attacking:  UpdateAttacking(); break;
            case AIState.Stagger:    UpdateStagger();   break;
            case AIState.Recover:    UpdateRecover();   break;
        }
    }

    // ── State updates ──────────────────────────────────────────

    void UpdateIdle()
    {
        if (_player != null && DistToPlayer() < aggroRange)
            EnterState(AIState.Approach);
    }

    void UpdateApproach()
    {
        if (_player == null) return;
        _agent.SetDestination(_player.position);

        if (DistToPlayer() <= attackRange)
        {
            _agent.ResetPath();
            if (_arena != null && _arena.RequestAttack(this))
                EnterState(AIState.Telegraph);
        }
    }

    void UpdateTelegraph()
    {
        FacePlayer();
        if (_stateTimer <= 0f)
            EnterState(AIState.Attacking);
    }

    void UpdateAttacking()
    {
        if (CurrentState != AIState.Attacking) return; // interrupted mid-attack

        FacePlayer();
        if (_stateTimer <= 0f)
        {
            _player?.GetComponent<PlayerCombatManager>()?.OnTakeHit();
            _arena?.ReleaseAttackSlot(this);
            EnterState(AIState.Recover);
        }
    }

    void UpdateStagger()
    {
        if (_stateTimer <= 0f)
            EnterState(AIState.Recover);
    }

    void UpdateRecover()
    {
        if (_stateTimer <= 0f)
            EnterState(DistToPlayer() < aggroRange ? AIState.Approach : AIState.Idle);
    }

    // ── Public API ─────────────────────────────────────────────

    public void OnCountered()
    {
        _arena?.ReleaseAttackSlot(this);
        EnterState(AIState.Stagger);
    }

    public void OnCounterMissed()
    {
        // Attack lands naturally in UpdateAttacking — nothing extra needed
    }

    // Called by BrawlerController (and future characters) when a hit lands
    public void TakeDamage(float amount)
    {
        if (CurrentState == AIState.Dead) return;

        Health -= amount;
        SetColor(Color.white);
        Invoke(nameof(RestoreStateColor), 0.08f);

        if (Health <= 0f)
        {
            EnterState(AIState.Dead);
            return; // early return so we don't also enter Stagger
        }

        // Interrupt the enemy whenever it takes a hit, regardless of state
        if (CurrentState == AIState.Telegraph || CurrentState == AIState.Attacking)
        {
            _arena?.ReleaseAttackSlot(this); // release so another enemy can attack
        }

        EnterState(AIState.Stagger);
    }

    // Called from DebugHUD to force a telegraph immediately
    public void DEBUG_ForceAttack()
    {
        if (CurrentState == AIState.Approach || CurrentState == AIState.Idle)
        {
            _agent.ResetPath();
            if (_arena != null && _arena.RequestAttack(this))
                EnterState(AIState.Telegraph);
        }
    }

    // ── Internal ───────────────────────────────────────────────

    void EnterState(AIState next)
    {
        CurrentState = next;
        switch (next)
        {
            case AIState.Idle:
                SetColor(_baseColor);
                _agent.isStopped = false;
                break;
            case AIState.Approach:
                SetColor(_baseColor);
                _agent.isStopped = false;
                break;
            case AIState.Telegraph:
                _stateTimer      = telegraphDuration;
                SetColor(telegraphColor);
                _agent.isStopped = true;
                break;
            case AIState.Attacking:
                _stateTimer      = attackDuration;
                SetColor(attackColor);
                break;
            case AIState.Stagger:
                _stateTimer      = staggerDuration;
                SetColor(staggerColor);
                _agent.isStopped = true;
                break;
            case AIState.Recover:
                _stateTimer      = recoverDuration;
                SetColor(Color.gray);
                _agent.isStopped = true;
                break;
            case AIState.Dead:
                SetColor(Color.black);
                _agent.isStopped = true;
                _arena?.ReleaseAttackSlot(this);
                break;
        }
    }

    // Restores the color that matches the current state after a white hit flash
    void RestoreStateColor()
    {
        switch (CurrentState)
        {
            case AIState.Telegraph: SetColor(telegraphColor); break;
            case AIState.Attacking: SetColor(attackColor);    break;
            case AIState.Stagger:   SetColor(staggerColor);   break;
            case AIState.Recover:   SetColor(Color.gray);     break;
            case AIState.Dead:      SetColor(Color.black);    break;
            default:                SetColor(_baseColor);     break;
        }
    }

    void FacePlayer()
    {
        if (_player == null) return;
        Vector3 dir = (_player.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    float DistToPlayer() =>
        _player == null ? float.MaxValue
                        : Vector3.Distance(transform.position, _player.position);

    void SetColor(Color c)
    {
        if (_renderer != null)
            _renderer.material.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}