using UnityEngine;

public class MovementTutorialTrigger : MonoBehaviour
{
    private void Start()
    {
        TutorialManager.Instance.Request(new TutorialStep
        {
            id          = "move_look",
            message     = "Beweeg met WASD · Kijk rond met de muis",
            keyIcons    = new[] { "WASD", "Mouse" },
            autoDismiss = false,
            dismissOnInput = true
        });
    }
}