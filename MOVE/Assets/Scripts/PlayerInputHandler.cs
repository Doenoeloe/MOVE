using UnityEngine;

[RequireComponent(typeof(PlayerCombatManager))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(CameraController))]
[RequireComponent(typeof(CharacterSwitchManager))]
public class PlayerInputHandler : MonoBehaviour
{
    private InputScheme            _input;
    private PlayerCombatManager    _combat;
    private PlayerMovement         _movement;
    private CameraController       _camera;
    private CharacterSwitchManager _switcher;

    void Awake()
    {
        _input    = new InputScheme();
        _combat   = GetComponent<PlayerCombatManager>();
        _movement = GetComponent<PlayerMovement>();
        _camera   = GetComponent<CameraController>();
        _switcher = GetComponent<CharacterSwitchManager>();
    }

    void OnEnable()
    {
        // Movement — feeds directly into PlayerMovement
        _input.Player.Walk.performed += ctx => _movement.SetMoveInput(ctx.ReadValue<Vector2>());
        _input.Player.Walk.canceled  += _   => _movement.SetMoveInput(Vector2.zero);

        // Combat
        _input.Player.Attack.performed   += _ => _combat.OnAttack();
        _input.Player.Counter.performed  += _ => _combat.OnCounter();
        _input.Player.Finisher.performed += _ => _combat.OnFinisher();

        // Camera lock-on
        _input.Player.LockOn.performed += _ => _camera.ToggleLockOn();

        // Character switching
        _input.Player.SwitchCharacter.performed += ctx =>
        {
            var key = ctx.control.name;
            if      (key == "1" || key == "dpadLeft")  _switcher.SwitchTo(0);
            else if (key == "2" || key == "dpadUp")    _switcher.SwitchTo(1);
            else if (key == "3" || key == "dpadRight") _switcher.SwitchTo(2);
        };

        _input.Player.Enable();
    }

    void OnDisable()
    {
        _input.Player.Disable();
    }
}