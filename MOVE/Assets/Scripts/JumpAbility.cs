using UnityEngine;

/// <summary>
/// Handles jumping. Attach alongside PlayerMovement.
/// Feeds jump velocity directly into PlayerMovement.VerticalVelocity.
/// Does NOT implement IMovementAbility — it reacts to an explicit input event
/// rather than overriding directional movement each frame.
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class JumpAbility : MonoBehaviour
{
    [Header("Jump")]
    public float jumpHeight = 2f;          // metres
    public int   extraJumps = 0;           // 0 = single jump, 1 = double jump, etc.

    [Header("Coyote / Buffer")]
    public float coyoteTime   = 0.12f;     // seconds after walking off a ledge where jump is still allowed
    public float jumpBuffer   = 0.12f;     // seconds a jump input is remembered before landing

    // ── state ──────────────────────────────────────────────────────────────
    private PlayerMovement _movement;
    private WallRunAbility _wallRun;       // optional — null-safe throughout

    private int   _jumpsRemaining;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool  _wasGrounded;

    // Derived from jump height + gravity:  v = sqrt(-2 * g * h)
    private float JumpVelocity =>
        Mathf.Sqrt(-2f * _movement.gravity * jumpHeight);

    // ── Unity ──────────────────────────────────────────────────────────────
    void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _wallRun  = GetComponent<WallRunAbility>(); // may be null
    }

    void Update()
    {
        TrackCoyoteTime();
        DrainBufferTimer();
        TryConsumeBuffer();
        
        if (_wallRun != null && !_movement.IsGrounded && !_wallRun.IsActive)
            _wallRun.TryActivate();
    }

    // ── Public API (called by PlayerInputHandler) ──────────────────────────

    /// <summary>Call this on Jump input performed.</summary>
    public void OnJumpPressed()
    {
        // Already wall running — launch off the wall
        if (_wallRun != null && _wallRun.IsActive)
        {
            _wallRun.LaunchFromWall();
            return;
        }

        if (CanJump())
        {
            ExecuteJump();

            // After leaving the ground, check if there's a wall to latch onto.
            // IsGrounded will be false this frame since ExecuteJump set upward velocity.
            if (_wallRun != null && !_movement.IsGrounded)
                _wallRun.TryActivate();
        }
        else
        {
            _jumpBufferTimer = jumpBuffer;
        }
    }

    /// <summary>Call this on Jump input released (for variable-height jumps).</summary>
    public void OnJumpReleased()
    {
        // Cut the jump short if still moving upward
        if (_movement.VerticalVelocity > 0f)
            _movement.VerticalVelocity *= 0.5f;
    }

    // ── Internal ──────────────────────────────────────────────────────────

    bool CanJump()
    {
        bool groundedOrCoyote = _movement.IsGrounded || _coyoteTimer > 0f;
        bool hasExtraJump     = _jumpsRemaining > 0;
        return groundedOrCoyote || hasExtraJump;
    }

    void ExecuteJump()
    {
        _movement.VerticalVelocity = JumpVelocity;

        bool usedCoyote = !_movement.IsGrounded && _coyoteTimer > 0f;
        if (!usedCoyote && !_movement.IsGrounded)
            _jumpsRemaining = Mathf.Max(0, _jumpsRemaining - 1);

        _coyoteTimer     = 0f;
        _jumpBufferTimer = 0f;
    }

    void TrackCoyoteTime()
    {
        bool grounded = _movement.IsGrounded;

        if (_wasGrounded && !grounded)
        {
            // Just left the ground — start coyote window and reset jumps
            _coyoteTimer    = coyoteTime;
            _jumpsRemaining = extraJumps;
        }
        else if (grounded)
        {
            // Landed — reset everything
            _jumpsRemaining = extraJumps;
            _coyoteTimer    = 0f;
        }
        else
        {
            _coyoteTimer = Mathf.Max(0f, _coyoteTimer - Time.deltaTime);
        }

        _wasGrounded = grounded;
    }

    void DrainBufferTimer()
    {
        if (_jumpBufferTimer > 0f)
            _jumpBufferTimer -= Time.deltaTime;
    }

    void TryConsumeBuffer()
    {
        if (_jumpBufferTimer > 0f && CanJump())
            ExecuteJump();
    }
}