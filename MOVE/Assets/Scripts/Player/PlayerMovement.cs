using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")] 
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotateSpeed = 720f; // degrees per second

    [Header("Physics")] 
    public float gravity = -20f;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    [Header("Root Motion")] 
    public bool useRootMotion = false;

    [Header("MovementAbilities")] 
    private IMovementAbility[] _movementAbilities;

    // Animator parameter names — match these in your Blend Tree
    static readonly int HashSpeed = Animator.StringToHash("Speed");
    static readonly int HashDirX = Animator.StringToHash("DirX");
    static readonly int HashDirY = Animator.StringToHash("DirY");

    public CharacterController _cc;
    public Animator _anim;

    public Camera _cam;

    private Vector2 _moveInput;
    private float _verticalVelocity;

    public float VerticalVelocity
    {
        get => _verticalVelocity;
        set => _verticalVelocity = value;
    }

    private bool _isGrounded;
    public bool IsGrounded => _isGrounded;
    public Vector3 GetCameraRelativeInput() => CameraRelativeInput();

    // Set externally by CameraController when lock-on activates
    [HideInInspector] public Transform LockOnTarget;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _anim = GetComponent<Animator>();

        _cam = Camera.main;
        _movementAbilities = GetComponents<IMovementAbility>();
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }

    // Called by PlayerInputHandler
    public void SetMoveInput(Vector2 input) => _moveInput = input;


    void Update()
    {
        CheckGround();
        ApplyGravity();

        if (!useRootMotion)
        {
            bool abilityConsumed = false;
            foreach (var ability in _movementAbilities)
            {
                if (ability.TryExecute(GetCameraRelativeInput(), Time.deltaTime))
                {
                    abilityConsumed = true;
                    break; // only one ability drives movement per frame
                }
            }
            if (!abilityConsumed)
                MoveAndRotate();
        }

        DriveAnimator();
    }

    // Called by Unity when root motion is enabled
    void OnAnimatorMove()
    {
        if (!useRootMotion) return;

        Vector3 rootDelta = _anim.deltaPosition;
        rootDelta.y = _verticalVelocity * Time.deltaTime;
        _cc.Move(rootDelta);

        if (_moveInput != Vector2.zero)
            RotateTowardInput();
    }

    // ── Internal ───────────────────────────────────────────────

    void MoveAndRotate()
    {
        Vector3 worldDir = CameraRelativeInput();
        float speed = _moveInput.magnitude > 0.5f ? runSpeed : walkSpeed;
        float mag = _moveInput.magnitude;

        Vector3 move = worldDir * (speed * mag);
        move.y = _verticalVelocity;
        _cc.Move(move * Time.deltaTime);

        if (LockOnTarget != null)
            RotateTowardTarget(LockOnTarget);
        else if (worldDir.sqrMagnitude > 0.01f)
            RotateTowardDir(worldDir);
    }

    void DriveAnimator()
    {
        if (_anim == null) return;

        float speed = _moveInput.magnitude;
        _anim.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);

        // Local-space direction for strafe blend trees (useful during lock-on)
        Vector3 localVel = transform.InverseTransformDirection(
            CameraRelativeInput() * speed);

        _anim.SetFloat(HashDirX, localVel.x, 0.1f, Time.deltaTime);
        _anim.SetFloat(HashDirY, localVel.z, 0.1f, Time.deltaTime);
    }

    Vector3 CameraRelativeInput()
    {
        if (_cam == null) return new Vector3(_moveInput.x, 0, _moveInput.y);

        Vector3 camForward = _cam.transform.forward;
        Vector3 camRight   = _cam.transform.right;
        camForward.y = 0; camForward.Normalize();
        camRight.y   = 0; camRight.Normalize();

        // Combine first, THEN normalize — preserves magnitude from input
        Vector3 dir = camForward * _moveInput.y + camRight * _moveInput.x;

        // Clamp to magnitude 1 max, but keep sub-1 magnitudes for analog stick walk/run
        float inputMag = Mathf.Min(_moveInput.magnitude, 1f);
        return dir.sqrMagnitude > 0.001f ? dir.normalized * inputMag : Vector3.zero;
    }

    void RotateTowardTarget(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0;
        if (dir == Vector3.zero) return;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotateSpeed * Time.deltaTime);
    }

    void RotateTowardDir(Vector3 dir)
    {
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotateSpeed * Time.deltaTime);
    }

    void RotateTowardInput()
    {
        Vector3 dir = CameraRelativeInput();
        if (dir.sqrMagnitude > 0.01f) RotateTowardDir(dir);
    }

    void CheckGround()
    {
        Vector3 spherePos = transform.position + Vector3.down * (_cc.height / 2f - _cc.radius);
        _isGrounded = Physics.CheckSphere(spherePos, _cc.radius + groundCheckDistance, groundLayer);
    }

    void ApplyGravity()
    {
        if (_isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f; // small snap to ground
        else
            _verticalVelocity += gravity * Time.deltaTime;
    }
}