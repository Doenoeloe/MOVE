using UnityEngine;

public class WorldMarker : MonoBehaviour
{
    public static WorldMarker Instance { get; private set; }

    [SerializeField] private GameObject markerPrefab;

    private GameObject _marker;
    private Transform  _target;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetTarget(Transform target)
    {
        if (_marker == null)
        {
            _marker = Instantiate(markerPrefab);
        }

        _target = target;

        if (target != null)
        {
            _marker.transform.position = target.position + Vector3.up * 2.5f;
            _marker.SetActive(true);
        }
        else
        {
            _marker.SetActive(false);
        }
    }

    public void Hide()
    {
        _target = null;
        if (_marker != null)
        {
            Destroy(_marker);
            _marker = null;
        }
    }
    
}