using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaveEncounterTrigger : MonoBehaviour
{
    [SerializeField] private WaveEncounterController _controller;

    private bool _triggered;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (other.GetComponentInParent<CharacterSwitchManager>() == null) return;

        _triggered = true;
        _controller.BeginEncounter();
        gameObject.SetActive(false);
    }
}