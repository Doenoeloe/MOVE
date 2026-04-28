using System.Collections;
using UnityEngine;

public class BrawlerController : CharacterBase, ICounterable, IFinisher
{
    [Header("Brawler Stats")]
    public float attackRange    = 1.6f;
    public float attackCooldown = 0.18f;
    public int   maxComboLength = 8;

    [Header("Hit Detection")]
    public Transform hitOrigin;
    public float     hitRadius  = 0.4f;
    public LayerMask enemyLayer;
    public float     baseDamage = 10f;

    [Header("Combo Scaling")]
    public float minAttackSpeed = 1.0f;
    public float maxAttackSpeed = 1.6f;

    public override float AttackRange    => attackRange;
    public bool           CanCounter     => true;
    public int            FinisherThreshold => 6;

    private Animator     _anim;
    private ComboTracker _combo;
    private float        _cooldownTimer;
    private int          _hitIndex;

    static readonly int HashComboIndex  = Animator.StringToHash("ComboIndex");
    static readonly int HashAttackSpeed = Animator.StringToHash("AttackSpeed");
    static readonly int HashCounter     = Animator.StringToHash("Counter");
    static readonly int HashFinisher    = Animator.StringToHash("Finisher");
    static readonly int HashStagger     = Animator.StringToHash("Stagger");

    static readonly string[] HitTriggers =
    {
        "Attack1","Attack2","Attack3","Attack4",
        "Attack5","Attack6","Attack7","Attack8"
    };

    // ── CharacterBase ──────────────────────────────────────────

    public override void OnActivated(CharacterSwitchManager mgr)
    {
        base.OnActivated(mgr);
        _anim  = GetComponent<Animator>();
        _combo = GetComponentInParent<ComboTracker>();

        Debug.Log($"[Brawler] OnActivated — _combo found: {_combo != null}, " +
                  $"_anim found: {_anim != null}");

        if (_combo != null)
        {
            _combo.resetTime         = 1.2f;
            _combo.finisherThreshold = FinisherThreshold;
        }
        else
        {
            Debug.LogError("[Brawler] ComboTracker NOT found in parent. " +
                           "Is ComboTracker on PlayerRoot?");
        }
    }

    public override void OnDeactivated()
    {
        _hitIndex      = 0;
        _cooldownTimer = 0f;
        Debug.Log("[Brawler] OnDeactivated.");
    }

    // ── IAttacker ──────────────────────────────────────────────

    public override void Attack(Transform target)
    {
        Debug.Log($"[Brawler] Attack() called. Cooldown remaining: {_cooldownTimer:F2}s");

        if (_cooldownTimer > 0f)
        {
            Debug.Log("[Brawler] Attack ignored — still in cooldown.");
            return;
        }

        FaceTarget(target);

        _hitIndex = (_hitIndex + 1) % maxComboLength;

        if (_anim != null)
        {
            _anim.SetInteger(HashComboIndex, _hitIndex);
            // Safe trigger index: _hitIndex is 1-based after increment, array is 0-based
            _anim.SetTrigger(HitTriggers[(_hitIndex - 1 + maxComboLength) % maxComboLength]);
            UpdateAnimatorSpeed();
        }

        Debug.Log($"[Brawler] Resolving hit on target: {target?.name}");
        ResolveHit(target);

        _cooldownTimer = attackCooldown;
    }

    // ── ICounterable ──────────────────────────────────────────

    public void Counter(Transform attacker)
    {
        Debug.Log($"[Brawler] Counter() called on attacker: {attacker?.name}");
        FaceTarget(attacker);

        if (_anim != null) _anim.SetTrigger(HashCounter);

        if (_combo != null)
        {
            _combo.RegisterHit();
            _combo.RegisterHit(); // Brawler bonus +2
        }

        attacker?.GetComponent<EnemyAI>()?.OnCountered();
    }

    // ── IFinisher ─────────────────────────────────────────────

    public void TriggerFinisher(Transform target)
    {
        Debug.Log($"[Brawler] Finisher triggered on: {target?.name}");
        FaceTarget(target);
        if (_anim != null) _anim.SetTrigger(HashFinisher);

        var enemy = target?.GetComponent<EnemyAI>();
        if (enemy != null) ApplyDamage(enemy, baseDamage * 4f);

        _hitIndex = 0;
    }

    public override void OnStagger()
    {
        _hitIndex = 0;
        if (_anim != null) _anim.SetTrigger(HashStagger);
    }

    // ── Internal ──────────────────────────────────────────────

    void ResolveHit(Transform preferredTarget)
    {
        // Try locked target first
        var enemy = preferredTarget?.GetComponent<EnemyAI>();
        if (enemy != null && IsInRange(preferredTarget))
        {
            Debug.Log($"[Brawler] Hit landed on {preferredTarget.name} " +
                      $"for {ComputeDamage():F1} damage.");
            ApplyDamage(enemy, ComputeDamage());
            return;
        }

        // Fallback overlap sphere
        if (hitOrigin == null)
        {
            Debug.LogWarning("[Brawler] hitOrigin is not assigned! " +
                             "Assign the HitOrigin Transform in the Inspector. " +
                             "Fallback hit detection skipped.");
            return;
        }

        var hits = Physics.OverlapSphere(hitOrigin.position, hitRadius, enemyLayer);
        Debug.Log($"[Brawler] Overlap sphere found {hits.Length} colliders " +
                  $"at {hitOrigin.position}, radius {hitRadius}, layer {enemyLayer.value}");

        foreach (var h in hits)
        {
            var e = h.GetComponent<EnemyAI>();
            if (e != null)
            {
                Debug.Log($"[Brawler] Fallback hit on {h.name} for {ComputeDamage():F1}");
                ApplyDamage(e, ComputeDamage());
            }
        }
    }

    float ComputeDamage()
    {
        float comboBonus = 1f + (_combo != null ? _combo.Count * 0.05f : 0f);
        return baseDamage * comboBonus;
    }

    void ApplyDamage(EnemyAI enemy, float amount)
    {
        enemy.TakeDamage(amount);
        // Tell PlayerCombatManager to increment the combo counter
        
        if (_combo != null)
            StartCoroutine(TrackEnemyStagger(enemy));
        
        var pcm = GetComponentInParent<PlayerCombatManager>();
        if (pcm != null)
        {
            Debug.Log("[Brawler] Calling OnHitLanded on PlayerCombatManager.");
            pcm.OnHitLanded();
        }
        else
        {
            Debug.LogError("[Brawler] PlayerCombatManager NOT found in parent! " +
                           "Combo will not increment. Is it on PlayerRoot?");
        }
    }

    void UpdateAnimatorSpeed()
    {
        if (_anim == null || _combo == null) return;
        float t     = (float)_combo.Count / maxComboLength;
        float speed = Mathf.Lerp(minAttackSpeed, maxAttackSpeed, t);
        _anim.SetFloat(HashAttackSpeed, speed);
    }

    bool IsInRange(Transform t) =>
        Vector3.Distance(transform.position, t.position) <= attackRange + 0.5f;

    void FaceTarget(Transform t)
    {
        if (t == null) return;
        Vector3 dir = (t.position - transform.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        if (hitOrigin == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
        Gizmos.DrawWireSphere(hitOrigin.position, hitRadius);
    }
    
    IEnumerator TrackEnemyStagger(EnemyAI enemy)
    {
        // Only freeze timer if this hit actually staggered them
        if (enemy.CurrentState != EnemyAI.AIState.Stagger) yield break;

        _combo.RegisterStagger();

        yield return new WaitUntil(() =>
            enemy == null ||
            (enemy.CurrentState != EnemyAI.AIState.Stagger &&
             enemy.CurrentState != EnemyAI.AIState.Recover));

        _combo.ReleaseStagger();
    }
}