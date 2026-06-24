using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    private readonly Queue<TutorialStep> _queue = new();
    private bool _showing;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool HasSeen(string id) => PlayerPrefs.GetInt($"tut_{id}", 0) == 1;

    public void Request(TutorialStep step)
    {
        Debug.Log($"[TutorialManager] Request: {step.id} — HasSeen: {HasSeen(step.id)} — _showing: {_showing}");
        if (HasSeen(step.id)) return;
        _queue.Enqueue(step);
        if (!_showing) ShowNext();
    }

    public void Dismiss(string id)
    {
        PlayerPrefs.SetInt($"tut_{id}", 1);
        PlayerPrefs.Save();
        _showing = false;

        TutorialOverlayUI.Instance.Hide(() =>
        {
            if (_queue.Count > 0) ShowNext();
        });
    }
    
    private void ShowNext()
    {
        _showing = true;
        var step = _queue.Dequeue();
        TutorialOverlayUI.Instance.Show(step);

        if (step.autoDismiss)
            StartCoroutine(AutoDismiss(step.id, 3f));
    }
    
    private IEnumerator AutoDismiss(string id, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Dismiss(id);
    }
}