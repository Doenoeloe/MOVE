using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossAreaTrigger : MonoBehaviour
{
    [SerializeField] private WaveEncounterController _controller;
    [SerializeField] private Transform EncounterTransform;
    private bool _triggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (other.GetComponentInParent<CharacterSwitchManager>() == null) return;
        QuestManager.Instance.CompleteObjective("reach_Boss");
        QuestManager.Instance.SetObjective(new QuestObjective
        {
            id                = "reach_boss",
            description       = "Vecht de golven aan vijanden! En versla de eind baas.",
            worldMarkerTarget = EncounterTransform
        });
        
        _triggered = true;
        _controller.BeginEncounter();
        gameObject.SetActive(false);
    }
}