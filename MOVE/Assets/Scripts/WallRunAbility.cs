using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class WallRunAbility : MonoBehaviour, IMovementAbility
{
    public enum WallRunMode { None, Side, Up }
    
    [Header("Detection")] 
    public float wallCheckDistance = 0.7f; // ray length from player centre

    public float
        wallMinParallelDot = 0.3f; // how parallel the player must face the wall (0 = perpendicular, 1 = fully parallel)

    public LayerMask wallLayer;

    [Header("Wall Run Feel")] public float wallRunSpeed = 7f;
    public float wallRunGravityScale = 0.15f; // fraction of normal gravity applied while running
    public float maxDuration = 2f; // seconds before the wall run expires
    
    [Header("Wall Up")]
    public float wallUpSpeed = 5f;
    public float wallUpMaxDuration = 1.2f;      // shorter than side run
    public float minEntryForwardSpeed = 3f;
    
    [Header("Launch")] public float wallJumpUpForce = 8f;
    public float wallJumpAwayForce = 6f; // pushes player away from the wall

    [Header("Camera Tilt")] public float tiltAngle = 12f; // degrees — fed to CameraController if present
    public float tiltSpeed = 5f;
    
    public bool IsActive => Mode != WallRunMode.None;
    public Vector3 WallNormal { get; private set; }
    public WallRunMode Mode { get; private set; }
    // ── Private ───────────────────────────────────────────────────────────
    private PlayerMovement _movement;
    private CameraController _camera; // optional tilt

    private float _runTimer;
    private int _wallSide; // -1 = left, +1 = right
    private float _currentTilt;

    private Vector3 _wallJumpHorizontalVelocity;

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _camera = GetComponent<CameraController>();
    }

    void Update()
    {
        // Expire by time or if we land
        if (IsActive)
        {
            _runTimer -= Time.deltaTime;
            if (_runTimer <= 0f || _movement.IsGrounded)
                Deactivate();
        }

        UpdateCameraTilt();
    }

    // ── IMovementAbility ──────────────────────────────────────────────────

    public bool TryExecute(Vector3 cameraRelativeInput, float deltaTime)
    {
        if (_wallJumpHorizontalVelocity.sqrMagnitude > 0.01f)
        {
            _movement._cc.Move(_wallJumpHorizontalVelocity * deltaTime);
            _wallJumpHorizontalVelocity = Vector3.Lerp(
                _wallJumpHorizontalVelocity, Vector3.zero, 2.5f * deltaTime);
        }

        if (!IsActive) return false;
        
        if (Mode == WallRunMode.Up)
            return ExecuteWallUp(deltaTime);
        else
            return ExecuteWallSide(deltaTime);
       
    }
    
    
    
    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by JumpAbility when the player jumps while airborne near a wall.
    /// Returns true if a wall was found and the run started.
    /// </summary>
    
    public bool TryActivate()
    {
        if (IsActive) return true;

        // Wall-up takes priority if running toward a wall
        if (TryDetectWallUp(out RaycastHit upHit))
        {
            ActivateUp(upHit);
            return true;
        }

        if (TryDetectWall(out RaycastHit sideHit, out int side))
        {
            Activate(sideHit, side);
            return true;
        }

        return false;
    }
    /// <summary>
    /// Called by JumpAbility when jump is pressed during an active wall run.
    /// Launches the player up and away from the wall.
    /// </summary>
    public void LaunchFromWall()
    {
        _movement.VerticalVelocity = wallJumpUpForce;

        // For wall-up, launch away from the wall face; for side, use the normal
        Vector3 awayDir = WallNormal; 

        _wallJumpHorizontalVelocity = awayDir * wallJumpAwayForce;
        
        if (Mode == WallRunMode.Up)
            transform.rotation = Quaternion.LookRotation(WallNormal);
        Deactivate();
    }

    /// <summary>Cancels the wall run without launching (e.g. fell away from wall).</summary>
    public void Cancel() => Deactivate();

    // ── Internal ──────────────────────────────────────────────────────────

    bool TryDetectWall(out RaycastHit hit, out int side)
    {
        RaycastHit rightHit, leftHit;

        if (Physics.Raycast(transform.position, transform.right, out rightHit, wallCheckDistance, wallLayer))
        {
            Vector3 wallForward = Vector3.Cross(rightHit.normal, Vector3.up);
            if (Mathf.Abs(Vector3.Dot(transform.forward, wallForward)) >= wallMinParallelDot)
            {
                hit = rightHit;
                side = 1;
                return true;
            }
        }

        if (Physics.Raycast(transform.position, -transform.right, out leftHit, wallCheckDistance, wallLayer))
        {
            Vector3 wallForward = Vector3.Cross(leftHit.normal, Vector3.up);
            if (Mathf.Abs(Vector3.Dot(transform.forward, wallForward)) >= wallMinParallelDot)
            {
                hit = leftHit;
                side = -1;
                return true;
            }
        }

        hit = default;
        side = 0;
        return false;
    }
    bool TryDetectWallUp(out RaycastHit hit)
    {
        if (!Physics.Raycast(transform.position, transform.forward, out hit, 
                wallCheckDistance, wallLayer))
            return false;

        // Must be moving fast enough toward the wall
        float forwardSpeed = Vector3.Dot(
            _movement._cc.velocity, transform.forward);
        return forwardSpeed >= minEntryForwardSpeed;
    }
    
    private bool ExecuteWallSide(float deltaTime)
    {
        // Re-validate wall is still there
        if (!TryDetectWall(out RaycastHit currentHit, out _))
        {
            Deactivate();
            return false;
        }

        WallNormal = currentHit.normal;

        // Move along the wall
        Vector3 wallForward = Vector3.Cross(WallNormal, Vector3.up) * _wallSide;
        // Ensure we travel in the same general direction the player is facing
        if (Vector3.Dot(wallForward, transform.forward) < 0f)
            wallForward = -wallForward;

        Vector3 move = wallForward * wallRunSpeed;
        move.y = _movement.VerticalVelocity; // gravity still applied (but scaled down)

        // Reduced gravity during wall run
        _movement.VerticalVelocity += _movement.gravity * wallRunGravityScale * deltaTime;
        _movement.VerticalVelocity = Mathf.Max(_movement.VerticalVelocity, -2f); // soft cap on fall speed during run

        _movement._cc.Move(move * deltaTime);

        // Keep the player facing along the wall
        if (wallForward != Vector3.zero)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(wallForward),
                720f * deltaTime);

        return true; // we consumed movement this frame
    }
    
    bool ExecuteWallUp(float deltaTime)
    {
        // Re-validate the wall is still ahead
        if (!Physics.Raycast(transform.position, transform.forward, 
                wallCheckDistance, wallLayer))
        {
            Deactivate();
            return false;
        }

        // Drive straight up, kill horizontal drift
        Vector3 move = Vector3.up * wallUpSpeed;
        _movement.VerticalVelocity = wallUpSpeed; // override gravity
        _movement._cc.Move(move * deltaTime);

        return true;
    }
    void Activate(RaycastHit hit, int side)
    {
        Mode       = WallRunMode.Side;
        WallNormal = hit.normal;
        _wallSide  = side;
        _runTimer  = maxDuration;
    }
    void ActivateUp(RaycastHit hit)
    {
        Mode       = WallRunMode.Up;
        WallNormal = hit.normal;
        _wallSide  = 0;
        _runTimer  = wallUpMaxDuration;
    }
    void Deactivate()
    {
        Mode       = WallRunMode.None;
        WallNormal = Vector3.zero;
        _runTimer  = 0f;
    }

    void UpdateCameraTilt()
    {
        if (_camera == null) return;

        float targetTilt = Mode == WallRunMode.Side ? tiltAngle * _wallSide : 0f;
        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, tiltSpeed * Time.deltaTime);
        _camera.SetRollOverride(_currentTilt);
    }
}