using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class VaultAbility : MonoBehaviour, IMovementAbility
{
    [Header("Detection")]
    public float vaultReachDistance = 1.2f;   // how far ahead to look
    public float vaultMaxHeight     = 1.5f;   // tallest obstacle to clear
    public float vaultMinHeight     = 0.5f;   // shortest obstacle (ignore tiny bumps)
    public LayerMask vaultableLayers;

    [Header("Motion")]
    public float vaultDuration      = 0.4f;   // seconds to complete vault
    public AnimationCurve vaultCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Animator trigger — set up a "Vault" trigger in your Animator
    static readonly int HashVault = Animator.StringToHash("Vault");

    public bool IsActive { get; private set; }

    private PlayerMovement _movement;
    private Coroutine      _vaultRoutine;

    void Awake() => _movement = GetComponent<PlayerMovement>();

    public bool TryExecute(Vector3 cameraRelativeInput, float deltaTime)
    {
        if (IsActive) return true; // keep consuming input during vault

        if (!ShouldVault(cameraRelativeInput, out Vector3 vaultTarget)) return false;

        _vaultRoutine = StartCoroutine(DoVault(vaultTarget));
        return true;
    }

    public void Cancel()
    {
        if (_vaultRoutine != null) StopCoroutine(_vaultRoutine);
        IsActive = false;
    }

    // ── Detection ─────────────────────────────────────────────

    bool ShouldVault(Vector3 inputDir, out Vector3 landPoint)
    {
        landPoint = Vector3.zero;
        if (inputDir.sqrMagnitude < 0.1f) return false;      // not moving
        if (!_movement.IsGrounded)         return false;      // must be grounded

        Vector3 origin    = transform.position + Vector3.up * vaultMinHeight;
        Vector3 direction = inputDir.normalized;

        // Ray from chest height forward — did we hit a wall/ledge face?
        if (!Physics.Raycast(origin, direction, out RaycastHit faceHit,
                             vaultReachDistance, vaultableLayers))
            return false;

        // Ray from above the obstacle downward — find the top surface
        Vector3 aboveObstacle = faceHit.point + direction * 0.1f
                                + Vector3.up * vaultMaxHeight;
        if (!Physics.Raycast(aboveObstacle, Vector3.down, out RaycastHit topHit,
                             vaultMaxHeight, vaultableLayers))
            return false;

        float obstacleHeight = topHit.point.y - transform.position.y;
        if (obstacleHeight < vaultMinHeight || obstacleHeight > vaultMaxHeight)
            return false;

        // Land point = just past the obstacle at ground level
        landPoint = topHit.point + direction * 0.6f;
        return true;
    }

    // ── Execution ─────────────────────────────────────────────

    IEnumerator DoVault(Vector3 target)
    {
        IsActive = true;
        _movement._anim?.SetTrigger(HashVault);

        Vector3 start   = transform.position;
        float   elapsed = 0f;

        // Compute a mid-air apex
        Vector3 apex = (start + target) / 2f + Vector3.up * 0.6f;

        while (elapsed < vaultDuration)
        {
            elapsed += Time.deltaTime;
            float t = vaultCurve.Evaluate(elapsed / vaultDuration);
            
            Vector3 pos = Mathf.Pow(1 - t, 2) * start
                        + 2 * (1 - t) * t     * apex
                        + Mathf.Pow(t, 2)     * target;

            _movement._cc.Move(pos - transform.position);
            yield return null;
        }

        _movement.VerticalVelocity = 0f;
        IsActive = false;
    }
}