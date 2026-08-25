using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private QuestObjective _current;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void SetObjective(QuestObjective objective)
    {
        if (PlayerPrefs.GetInt($"quest_{objective.id}", 0) == 1) return;
    
        _current = objective;
        QuestHUD.Instance.Show(objective);
        WorldMarker.Instance.Hide();
        WorldMarker.Instance.SetTarget(objective.worldMarkerTarget);
    }

    public void CompleteObjective(string id)
    {
        if (_current == null || _current.id != id) return;
    
        PlayerPrefs.SetInt($"quest_{id}", 1);
        PlayerPrefs.Save();
        _current = null;
        QuestHUD.Instance.Hide();
        WorldMarker.Instance.Hide();
    }
}