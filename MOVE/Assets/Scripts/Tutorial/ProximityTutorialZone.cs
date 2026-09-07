using UnityEngine;

public class ProximityTutorialZone : MonoBehaviour
{
    [SerializeField] private string stepId;
    [SerializeField] private string message;
    [SerializeField] private string[] keyIcons;
    [SerializeField] private Transform combatAreaTransform;
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ProximityTutorial] {other.gameObject.name}");
        if (other.GetComponent<CharacterSwitchManager>() == null) return;

        TutorialManager.Instance.Request(new TutorialStep
        {
            id       = stepId,
            message  = message,
            keyIcons = keyIcons,
            autoDismiss    = true,
            dismissOnInput = false
        });
        
        QuestManager.Instance.CompleteObjective("reach_wall");

        QuestManager.Instance.SetObjective(new QuestObjective
        {
            id                = "reach_combat",
            description       = "Bereik het gevechtsgebied",
            worldMarkerTarget = combatAreaTransform
        });
    }
}