using UnityEngine;

public class MovementTutorialTrigger : MonoBehaviour
{
    [SerializeField] private Transform wallClimbTransform;
    
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
        
        QuestManager.Instance.SetObjective(new QuestObjective
        {
            id                = "reach_wall",
            description       = "Bereik de klimwand",
            worldMarkerTarget = wallClimbTransform
        });
    }
}