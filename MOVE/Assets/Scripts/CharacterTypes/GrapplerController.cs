using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrapplerController : CharacterBase, IFinisher
{
    // ── Inspector ──────────────────────────────────────────────

    [Header("Grappler Stats")]
    public float grabRange      = 2.4f;
    public float attackCooldown = 0.45f;
    public int   maxComboLength = 4;

    [Header("Grab Detection")]
    public Transform grabOrigin;
    public float     grabRadius    = 0.8f;
    public LayerMask enemyLayer;
    public float     baseDamage    = 22f; 

    [Header("Knockback")]
    public float knockbackForce    = 6f;
    public float knockbackDuration = 0.25f;

    [Header("Counter")]
    public float counterThrowForce   = 10f;
    public float counterThrowRadius  = 3f; 
    [Header("Pile Driver")]
    public float pileDriveAOERadius  = 4f;
    public float pileDriveMultiplier = 5f;
    
    public override float AttackRange   => grabRange;
    
    public int FinisherThreshold => 4;

    private Animator     _anim;
    private ComboTracker _combo;
    private float        _cooldownTimer;
    private int          _hitIndex;

    static readonly int HashComboIndex = Animator.StringToHash("ComboIndex");
    static readonly int HashCounterGrab= Animator.StringToHash("CounterGrab");
    static readonly int HashThrow      = Animator.StringToHash("Throw");
    static readonly int HashPileDriver = Animator.StringToHash("PileDriver");
    static readonly int HashStagger    = Animator.StringToHash("Stagger");

    static readonly string[] GrabTriggers = { "Grab1","Grab2","Grab3","Grab4" };
    
    public override void OnActivated(CharacterSwitchManager mgr, SharedCharacterState state)
    {
        base.OnActivated(mgr, state);
        _anim  = GetComponent<Animator>();
        _combo = GetComponentInParent<ComboTracker>();

        Debug.Log($"[Grappler] OnActivated — _combo: {_combo != null}, _anim: {_anim != null}");

        if (_combo != null)
        {
            _combo.resetTime         = 1.8f;
            _combo.finisherThreshold = FinisherThreshold;
        }
    }

    public override void OnDeactivated()
    {
        _hitIndex      = 0;
        _cooldownTimer = 0f;
        Debug.Log("[Grappler] OnDeactivated.");
    }

    public override void OnStagger()
    {
        _hitIndex = 0;
        if (_anim != null) _anim.SetTrigger(HashStagger);
        Debug.Log("[Grappler] Staggered.");
    }

    public override void OnEnemyAttackLanded(Transform attacker)
    {
        AbsorbAndThrow(attacker);
    }

    public override void Attack(Transform target)
    {
        Debug.Log($"[Grappler] Attack() — cooldown: {_cooldownTimer:F2}s");

        if (_cooldownTimer > 0f) return;

        PullTowardTarget(target);
        FaceTarget(target);

        _hitIndex = (_hitIndex + 1) % maxComboLength;

        if (_anim != null)
        {
            _anim.SetInteger(HashComboIndex, _hitIndex);
            _anim.SetTrigger(GrabTriggers[(_hitIndex - 1 + maxComboLength) % maxComboLength]);
        }

        ResolveGrab(target);
        _cooldownTimer = attackCooldown;
    }

    public void TriggerFinisher(Transform target)
    {
        Debug.Log($"[Grappler] Pile Driver on: {target?.name}");
        FaceTarget(target);

        if (_anim != null) _anim.SetTrigger(HashPileDriver);

        StartCoroutine(PileDriveRoutine(target));
        _hitIndex = 0;
    }

    public void AbsorbAndThrow(Transform attacker)
    {
        Debug.Log($"[Grappler] Absorbed hit from {attacker?.name} — throwing them.");

        if (_anim != null)
        {
            _anim.SetTrigger(HashCounterGrab);
            _anim.SetTrigger(HashThrow);
        }

        StartCoroutine(ThrowRoutine(attacker));
        
        SharedState?.TakeDamage(3f);
    }

    void ResolveGrab(Transform preferredTarget)
    {
        var enemy = preferredTarget?.GetComponent<EnemyAI>();
        if (enemy != null && IsInRange(preferredTarget))
        {
            Debug.Log($"[Grappler] Grab landed on {preferredTarget.name} " +
                      $"for {ComputeDamage():F1} damage.");
            ApplyGrab(enemy, preferredTarget, ComputeDamage());
            return;
        }
        
        if (grabOrigin == null)
        {
            Debug.LogWarning("[Grappler] grabOrigin not assigned — fallback skipped.");
            return;
        }

        var hits = Physics.OverlapSphere(grabOrigin.position, grabRadius, enemyLayer);
        Debug.Log($"[Grappler] Overlap sphere found {hits.Length} colliders.");
        foreach (var h in hits)
        {
            var e = h.GetComponent<EnemyAI>();
            if (e != null) ApplyGrab(e, h.transform, ComputeDamage());
        }
    }

    void ApplyGrab(EnemyAI enemy, Transform enemyTransform, float damage)
    {
        enemy.TakeDamage(damage);
        ApplyKnockback(enemyTransform);

        var pcm = GetComponentInParent<PlayerCombatManager>();
        if (pcm != null)
            pcm.OnHitLanded();
        else
            Debug.LogError("[Grappler] PlayerCombatManager not found in parent!");
    }

    void ApplyKnockback(Transform enemyTransform)
    {
        // Push enemy away from player
        Vector3 dir = (enemyTransform.position - transform.position).normalized;
        dir.y = 0.3f; // slight upward arc
        var agent = enemyTransform.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
            StartCoroutine(KnockbackRoutine(agent, dir * knockbackForce));
    }

    IEnumerator KnockbackRoutine(UnityEngine.AI.NavMeshAgent agent, Vector3 force)
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) yield break;

        agent.isStopped = true;
        float elapsed = 0f;
        var   tf      = agent.transform;

        while (elapsed < knockbackDuration)
        {
            // Agent may have been disabled by death state mid-knockback
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) yield break;

            tf.position += force * Time.deltaTime;
            elapsed     += Time.deltaTime;
            yield return null;
        }

        // Final guard before resuming — death may have fired during last frame
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) yield break;

        var enemy = tf.GetComponent<EnemyAI>();
        if (enemy != null && enemy.CurrentState != EnemyAI.AIState.Dead)
            agent.isStopped = false;
    }

    IEnumerator ThrowRoutine(Transform attacker)
    {
        // Brief pause — grab animation plays
        yield return new WaitForSeconds(0.3f);

        if (attacker == null) yield break;

        var attackerEnemy = attacker.GetComponent<EnemyAI>();
        if (attackerEnemy != null && attackerEnemy.CurrentState == EnemyAI.AIState.Dead)
            yield break;

        FaceTarget(attacker);

        Vector3 throwDir   = FindBestThrowDirection(attacker);
        Vector3 throwStart = attacker.position;
        float   throwDist  = counterThrowForce * knockbackDuration;

        // SphereCast along the throw path to find enemies actually in the way
        // This is what makes the throw feel like a real body flying through a crowd
        var chainHits = new List<EnemyAI>();
        RaycastHit[] pathHits = Physics.SphereCastAll(
            throwStart,
            0.5f,           // radius of the "body" flying through air
            throwDir,
            throwDist,
            enemyLayer);

        foreach (var hit in pathHits)
        {
            if (hit.transform == attacker) continue;
            var e = hit.transform.GetComponent<EnemyAI>();
            if (e != null && e.CurrentState != EnemyAI.AIState.Dead)
                chainHits.Add(e);
        }

        Debug.Log($"[Grappler] Throw path cast found {chainHits.Count} enemies in the way.");

        // Move the attacker along the throw path
        var agent = attacker.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
            yield return StartCoroutine(KnockbackRoutine(agent, throwDir * counterThrowForce));

        // Apply damage to every enemy the thrown body passed through
        foreach (var e in chainHits)
        {
            if (e == null) continue;
            Debug.Log($"[Grappler] Chain throw hit {e.name} for {baseDamage * 0.6f:F1}");
            e.TakeDamage(baseDamage * 0.6f);
            ApplyKnockback(e.transform);
        }

        // Register combo hits
        var pcm = GetComponentInParent<PlayerCombatManager>();
        pcm?.OnHitLanded();
        pcm?.OnHitLanded();
    }

    IEnumerator PileDriveRoutine(Transform target)
    {
        // Brief wind-up
        yield return new WaitForSeconds(0.4f);

        if (target == null) yield break;

        // Enemy may have died during pile driver wind-up
        var primaryCheck = target.GetComponent<EnemyAI>();
        if (primaryCheck != null && primaryCheck.CurrentState == EnemyAI.AIState.Dead)
            yield break;

        var primaryEnemy = target.GetComponent<EnemyAI>();
        if (primaryEnemy != null)
            primaryEnemy.TakeDamage(baseDamage * pileDriveMultiplier);

        Debug.Log($"[Grappler] Pile Driver AOE at {target.position}, radius {pileDriveAOERadius}");

        // AOE slam — damages all enemies in radius
        var hits = Physics.OverlapSphere(target.position, pileDriveAOERadius, enemyLayer);
        foreach (var h in hits)
        {
            if (h.transform == target) continue; // already hit primary
            var e = h.GetComponent<EnemyAI>();
            if (e != null)
            {
                float dist     = Vector3.Distance(target.position, h.transform.position);
                float falloff  = 1f - (dist / pileDriveAOERadius); // closer = more damage
                float aoeHit   = baseDamage * pileDriveMultiplier * falloff;
                Debug.Log($"[Grappler] AOE hit {h.name} for {aoeHit:F1}");
                e.TakeDamage(aoeHit);
                ApplyKnockback(h.transform);
            }
        }
    }

    Vector3 FindBestThrowDirection(Transform attacker)
    {
        // Find the direction with the most enemies — throw the attacker into them
        var candidates = Physics.OverlapSphere(transform.position, 6f, enemyLayer);
        Vector3 bestDir   = transform.forward;
        int     bestCount = 0;

        foreach (var c in candidates)
        {
            if (c.transform == attacker) continue;
            Vector3 dir   = (c.transform.position - transform.position).normalized;
            int     count = 0;

            // Count how many enemies are roughly in this direction
            foreach (var other in candidates)
            {
                if (other.transform == attacker) continue;
                if (Vector3.Dot(dir, (other.transform.position - transform.position).normalized) > 0.7f)
                    count++;
            }

            if (count > bestCount) { bestCount = count; bestDir = dir; }
        }

        bestDir.y = 0.2f;
        return bestDir.normalized;
    }

    float ComputeDamage()
    {
        float comboBonus = 1f + (_combo != null ? _combo.Count * 0.08f : 0f);
        return baseDamage * comboBonus;
    }

    bool IsInRange(Transform t) =>
        Vector3.Distance(transform.position, t.position) <= grabRange + 0.6f;

    void PullTowardTarget(Transform target)
    {
        // Move PlayerRoot via CharacterController — never touch child transform directly
        // or the camera loses track of the player
        var cc = GetComponentInParent<CharacterController>();
        if (cc == null) return;

        Vector3 dir  = (target.position - cc.transform.position).normalized;
        dir.y        = 0f;
        float dist   = Vector3.Distance(cc.transform.position, target.position);

        // Only step forward if we are farther than half grab range
        if (dist > grabRange * 0.5f)
        {
            float step = Mathf.Min(1.2f, dist - 1f);
            cc.Move(dir * step);
        }
    }

    void FaceTarget(Transform t)
    {
        if (t == null) return;

        // Rotate PlayerRoot, not the child Grappler GameObject,
        // so the camera forward stays consistent
        Transform root = GetComponentInParent<CharacterController>()?.transform
                         ?? transform.parent
                         ?? transform;

        Vector3 dir = (t.position - root.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
            root.rotation = Quaternion.LookRotation(dir);
    }

    void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        // Grab range
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, grabRange);

        // Pile driver AOE
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, pileDriveAOERadius);

        // Counter throw radius
        if (grabOrigin != null)
        {
            Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
            Gizmos.DrawWireSphere(grabOrigin.position, counterThrowRadius);
        }
    }
}