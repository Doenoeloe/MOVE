using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Cinemachine Cameras (3.x)")]
    public CinemachineCamera freeCam;   // your existing orbital cam, priority 10
    public CinemachineCamera lockCam;   // second cam for lock-on,    priority 11 when active

    [Header("Lock-on")]
    public float lockOnSearchRadius = 12f;
    public LayerMask enemyLayer;
    public float lockCamDistance    = 5f;
    public float lockCamHeight      = 1.6f;

    private PlayerMovement  _movement;
    private TargetingSystem _targeting;

    private Transform _lockTarget;
    private bool      _isLocked;
    
    private float _rollOverride;

    void Awake()
    {
        _movement  = GetComponent<PlayerMovement>();
        _targeting = GetComponent<TargetingSystem>();
        SetLockOn(false);
    }

    void Update()
    {
        if (!_isLocked) return;

        if (_lockTarget == null || !_lockTarget.gameObject.activeInHierarchy)
        {
            SetLockOn(false);
            return;
        }

        PositionLockCamera();
    }

    public bool IsLocked        => _isLocked;
    public Transform LockTarget => _lockTarget;

    // Called by PlayerInputHandler
    public void ToggleLockOn()
    {
        if (_isLocked) { SetLockOn(false); return; }

        Transform target = FindBestLockTarget();
        if (target == null) return;

        _lockTarget = target;
        SetLockOn(true);
    }

    /// <summary>
    /// Called by WallRunAbility each frame to tilt the camera into the wall.
    /// Pass 0 to reset. WallRunAbility handles the lerp on its end so this
    /// just needs to apply whatever value it receives.
    /// </summary>
    public void SetRollOverride(float degrees)
    {
        _rollOverride = degrees;
        ApplyRoll();
    }

    void SetLockOn(bool locked)
    {
        _isLocked = locked;

        if (freeCam != null) freeCam.Priority  = locked ? 10 : 11;
        if (lockCam != null) lockCam.Priority  = locked ? 11 : 10;

        if (_movement != null)
            _movement.LockOnTarget = locked ? _lockTarget : null;

        if (_targeting != null)
        {
            if (locked) _targeting.SetOverrideTarget(_lockTarget);
            else        _targeting.ClearOverrideTarget();
        }

        if (!locked) _lockTarget = null;
    }

    void PositionLockCamera()
    {
        if (lockCam == null || _lockTarget == null) return;

        Vector3 toTarget = (_lockTarget.position - transform.position).normalized;
        toTarget.y = 0;

        Vector3 camPos = transform.position
                         - toTarget * lockCamDistance
                         + Vector3.up * lockCamHeight;

        lockCam.transform.position = camPos;
        lockCam.transform.LookAt(
            Vector3.Lerp(transform.position, _lockTarget.position, 0.5f)
            + Vector3.up * 0.8f);

        // Apply roll on top of the look-at rotation
        ApplyRollToTransform(lockCam.transform);
    }

    // ── Roll helpers ──────────────────────────────────────────────────────

    void ApplyRoll()
    {
        // Apply to whichever camera is currently active
        Transform camTransform = _isLocked && lockCam != null
            ? lockCam.transform
            : freeCam != null ? freeCam.transform : null;

        if (camTransform != null)
            ApplyRollToTransform(camTransform);
    }

    /// Rotates the camera transform around its own forward axis by _rollOverride degrees.
    /// Preserves the existing position and forward direction — only the roll changes.
    void ApplyRollToTransform(Transform camTransform)
    {
        if (Mathf.Approximately(_rollOverride, 0f)) return;

        camTransform.rotation = camTransform.rotation *
            Quaternion.AngleAxis(_rollOverride, Vector3.forward);
    }

    Transform FindBestLockTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, lockOnSearchRadius, enemyLayer);

        Transform best      = null;
        float     bestScore = float.MaxValue;

        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyAI>();
            if (enemy == null || !enemy.IsTargetable) continue;

            float dist  = Vector3.Distance(transform.position, hit.transform.position);
            float angle = Vector3.Angle(transform.forward,
                              hit.transform.position - transform.position);

            float score = dist + angle * 0.08f;
            if (score < bestScore) { bestScore = score; best = hit.transform; }
        }

        return best;
    }
}