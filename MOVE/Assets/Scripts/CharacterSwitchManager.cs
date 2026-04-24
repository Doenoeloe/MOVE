using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSwitchManager : MonoBehaviour
{
    [Header("Characters")]
    public GameObject[] characters;       // Assign Brawler, Grappler, WeaponUser in Inspector
    public int activeIndex = 0;

    [Header("Shared State")]
    public float health = 100f;
    public int comboCount = 0;

    private InputScheme _input;

    void Awake()
    {
        _input = new InputScheme();
    }

    void OnEnable()
    {
        _input.Player.SwitchCharacter.performed += ctx =>
        {
            
            var key = ctx.control.name;
            if      (key == "1" || key == "left") SwitchTo(0);
            else if (key == "2" || key == "up") SwitchTo(1);
            else if (key == "3" || key == "right") SwitchTo(2);
        };
        _input.Player.Enable();
    }

    void OnDisable()
    {
        _input.Player.Disable();
    }

    void Start()
    {
        SwitchTo(0); // Activate first character on start
    }

    public void SwitchTo(int index)
    {
        if (index == activeIndex) return;
        if (index < 0 || index >= characters.Length) return;

        // Save outgoing character's local state if needed
        var outgoing = characters[activeIndex].GetComponent<CharacterBase>();
        outgoing?.OnDeactivated();

        // Swap active
        characters[activeIndex].SetActive(false);
        activeIndex = index;
        characters[activeIndex].SetActive(true);

        // Give incoming character the shared state
        var incoming = characters[activeIndex].GetComponent<CharacterBase>();
        incoming?.OnActivated(this);
    }
}