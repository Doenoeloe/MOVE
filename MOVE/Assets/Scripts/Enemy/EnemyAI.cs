using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyVisuals))]
[RequireComponent(typeof(EnemyNavigator))]
public class EnemyAI : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────

    [Header("Detection")] public float aggroRange = 10f;
    public float attackRange = 1.8f;

    [Header("Timing")] public float telegraphDuration = 0.6f;
    public float attackDuration = 0.4f;
    public float recoverDuration = 1.0f;
    public float staggerDuration = 1.2f;

    public enum AIState
    {
        Idle,
        Approach,
        Telegraph,
        Attacking,
        Stagger,
        Recover,
        Dead
    }

    public AIState CurrentState { get; private set; } = AIState.Idle;
    
    public bool IsHittable => CurrentState != AIState.Dead;
    
    public bool IsTargetable => CurrentState != AIState.Stagger
                                && CurrentState != AIState.Dead;


    private EnemyHealth _health;
    private EnemyVisuals _visuals;
    private EnemyNavigator _nav;
    private IAttackSlotProvider _slots;
    private Transform _player;
    private float _stateTimer;

    void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _visuals = GetComponent<EnemyVisuals>();
        _nav = GetComponent<EnemyNavigator>();

        _health.OnDied += HandleDeath;
        _health.OnDamageTaken += HandleDamageTaken;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDied -= HandleDeath;
            _health.OnDamageTaken -= HandleDamageTaken;
        }
    }

    void Update()
    {
        if (CurrentState == AIState.Dead) return;

        _stateTimer -= Time.deltaTime;

        switch (CurrentState)
        {
            case AIState.Idle: UpdateIdle(); break;
            case AIState.Approach: UpdateApproach(); break;
            case AIState.Telegraph: UpdateTelegraph(); break;
            case AIState.Attacking: UpdateAttacking(); break;
            case AIState.Stagger: UpdateStagger(); break;
            case AIState.Recover: UpdateRecover(); break;
        }
    }

    public void SetSlotProvider(IAttackSlotProvider provider) => _slots = provider;

    public void TakeDamage(float amount) => _health.TakeDamage(amount);

    public void OnCountered()
    {
        _slots?.ReleaseAttackSlot(this);
        EnterState(AIState.Stagger);
    }

    public void OnCounterMissed()
    {
    }

    public void DEBUG_ForceAttack()
    {
        if (CurrentState != AIState.Approach && CurrentState != AIState.Idle) return;

        _nav.Stop();
        if (_slots != null && _slots.RequestAttack(this))
            EnterState(AIState.Telegraph);
    }

    void UpdateIdle()
    {
        if (_player != null && _nav.DistanceTo(_player) < aggroRange)
            EnterState(AIState.Approach);
    }

    void UpdateApproach()
    {
        if (_player == null) return;

        _nav.MoveTo(_player.position);

        if (_nav.DistanceTo(_player) <= attackRange)
        {
            _nav.Stop();
            if (_slots != null && _slots.RequestAttack(this))
                EnterState(AIState.Telegraph);
        }
    }

    void UpdateTelegraph()
    {
        _nav.FaceTarget(_player);
        if (_stateTimer <= 0f)
            EnterState(AIState.Attacking);
    }

    void UpdateAttacking()
    {
        _nav.FaceTarget(_player);

        if (_stateTimer <= 0f)
        {
            var switcher = _player?.GetComponent<CharacterSwitchManager>();
            var activeGO = switcher?.GetActiveCharacter();
            activeGO?.GetComponent<IHittable>()?.OnEnemyAttackLanded(transform);

            _slots?.ReleaseAttackSlot(this);
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
            EnterState(_nav.DistanceTo(_player) < aggroRange ? AIState.Approach : AIState.Idle);
    }

    void HandleDeath()
    {
        EnterState(AIState.Dead);
    }

    void HandleDamageTaken(float remainingHp)
    {
        _visuals.FlashHit();

        if (CurrentState == AIState.Telegraph || CurrentState == AIState.Attacking)
            _slots?.ReleaseAttackSlot(this);

        EnterState(AIState.Stagger);
    }

    void EnterState(AIState next)
    {
        CurrentState = next;
        _visuals.SetForState(next);

        switch (next)
        {
            case AIState.Idle:
                _nav.ResumeIdle();
                break;

            case AIState.Approach:
                _nav.ResumeIdle();
                break;

            case AIState.Telegraph:
                _stateTimer = telegraphDuration;
                _nav.Stop();
                break;

            case AIState.Attacking:
                _stateTimer = attackDuration;
                break;

            case AIState.Stagger:
                _stateTimer = staggerDuration;
                _nav.Stop();
                break;

            case AIState.Recover:
                _stateTimer = recoverDuration;
                _nav.Stop();
                break;

            case AIState.Dead:
                _slots?.ReleaseAttackSlot(this);
                _nav.Disable();
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}