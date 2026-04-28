using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRadius = 8f;
    public float lockOnRadius    = 12f;
    public LayerMask enemyLayer;
    public float faceSpeed = 720f;

    public Transform CurrentTarget { get; private set; }
    public bool HasTarget => CurrentTarget != null;

    private bool      _hardLocked;
    private Transform _overrideTarget;

    void Update()
    {
        Transform faceMe = _overrideTarget ?? CurrentTarget;
        if (_hardLocked && faceMe != null)
            FaceTarget(faceMe);

        ValidateCurrentTarget();
    }

    public Transform AcquireTarget()
    {
        if (CurrentTarget != null) return CurrentTarget;
        CurrentTarget = FindBestTarget(hittableOnly: true);
        _hardLocked   = CurrentTarget != null;
        return CurrentTarget;
    }

    public Transform PeekNearest()
    {
        return FindBestTarget();
    }
    
    public void SetOverrideTarget(Transform target)
    {
        _overrideTarget = target;
    }
    
    public void ClearOverrideTarget()
    {
        _overrideTarget = null;
    }

    public void ClearTarget()
    {
        CurrentTarget   = null;
        _overrideTarget = null;
        _hardLocked     = false;
    }
    
    Transform FindBestTarget(bool hittableOnly = false)
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, detectionRadius, enemyLayer);

        Transform best      = null;
        float     bestScore = float.MaxValue;

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy == null) continue;

            // Use IsHittable for attacks, IsTargetable for lock-on
            bool valid = hittableOnly ? enemy.IsHittable : enemy.IsTargetable;
            if (!valid) continue;

            float dist  = Vector3.Distance(transform.position, hit.transform.position);
            float angle = Vector3.Angle(
                transform.forward,
                hit.transform.position - transform.position);

            float score = dist + (angle * 0.08f);
            if (score < bestScore) { bestScore = score; best = hit.transform; }
        }

        return best;
    }

    void FaceTarget(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion desired = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, desired, faceSpeed * Time.deltaTime);
    }

    void ValidateCurrentTarget()
    {
        if (CurrentTarget == null) return;

        float dist  = Vector3.Distance(transform.position, CurrentTarget.position);
        var   enemy = CurrentTarget.GetComponent<EnemyAI>();

        // Use IsHittable here — keep targeting staggered enemies
        if (dist > lockOnRadius || enemy == null || !enemy.IsHittable)
            ClearTarget();
    }
}