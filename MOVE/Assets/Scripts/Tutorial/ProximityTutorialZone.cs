using UnityEngine;

public class ProximityTutorialZone : MonoBehaviour
{
    [SerializeField] private string stepId;
    [SerializeField] private string message;
    [SerializeField] private string[] keyIcons;
    
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
    }
}