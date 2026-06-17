using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Beheert NavMeshAgent movement. Los component zodat het hergebruikt kan worden
/// zonder de state machine te hoeven aanpassen.
/// Hergebruikt op: MeleeEnemy, RangedEnemy, ShieldEnemy, BossEnemy
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavigator : MonoBehaviour
{
    private NavMeshAgent _agent;

    void Awake() => _agent = GetComponent<NavMeshAgent>();

    public void MoveTo(Vector3 destination)
    {
        if (!IsUsable()) return;
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (!IsUsable()) return;
        _agent.isStopped = true;
    }

    public void ResumeIdle()
    {
        if (!IsUsable()) return;
        _agent.isStopped = false;
    }

    public void Disable()
    {
        if (_agent.enabled) _agent.ResetPath();
        _agent.enabled = false;
    }

    public void FaceTarget(Transform target)
    {
        if (target == null) return;
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    public float DistanceTo(Transform target)
    {
        return target == null
            ? float.MaxValue
            : Vector3.Distance(transform.position, target.position);
    }

    bool IsUsable() => _agent.enabled && _agent.isOnNavMesh;

    void OnDrawGizmosSelected()
    {
        if (_agent == null || !_agent.hasPath) return;
        Gizmos.color = Color.cyan;
        var corners = _agent.path.corners;
        for (int i = 0; i < corners.Length - 1; i++)
            Gizmos.DrawLine(corners[i], corners[i + 1]);
    }
}