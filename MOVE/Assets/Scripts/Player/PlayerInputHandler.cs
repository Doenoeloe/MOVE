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
    private JumpAbility            _jump;
    private ActiveSkillManager     _activeSkills;
    void Awake()
    {
        _input    = new InputScheme();
        _combat   = GetComponent<PlayerCombatManager>();
        _movement = GetComponent<PlayerMovement>();
        _camera   = GetComponent<CameraController>();
        _switcher = GetComponent<CharacterSwitchManager>();
        _jump     = GetComponent<JumpAbility>();  
        _activeSkills = GetComponent<ActiveSkillManager>();
    }

    void OnEnable()
    {
        // Movement — feeds directly into PlayerMovement
        _input.Player.Walk.performed += ctx =>
        {
            _movement.SetMoveInput(ctx.ReadValue<Vector2>());
            TutorialOverlayUI.Instance?.NotifyMovementInput();
        };
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
        
        _input.Player.Jump.performed += _ => _jump.OnJumpPressed();
        _input.Player.Jump.canceled  += _ => _jump.OnJumpReleased();
        
        _input.Player.SkillSlot1.performed += _ => _activeSkills?.TriggerSlot(KeyCode.Q);
        _input.Player.SkillSlot2.performed += _ => _activeSkills?.TriggerSlot(KeyCode.E);
        
        _input.Player.Enable();
    }

    void OnDisable()
    {
        _input.Player.Disable();
    }
}