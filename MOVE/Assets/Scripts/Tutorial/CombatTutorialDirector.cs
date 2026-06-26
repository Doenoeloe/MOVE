using System.Collections;
using UnityEngine;

public class CombatTutorialDirector : MonoBehaviour
{
    public static CombatTutorialDirector Instance { get; private set; }

    [SerializeField] private float slowScale    = 0.25f;
    [SerializeField] private float slowDuration = 3f;
    private bool _isRunning;
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnEnemyBeginAttack()
    {
        if (_isRunning) return;
        if (TutorialManager.Instance.HasSeen("attack_prompt") &&
            TutorialManager.Instance.HasSeen("lockon_prompt") &&
            TutorialManager.Instance.HasSeen("parry_prompt")) return;

        StartCoroutine(SlowAndExplain());
    }

    private IEnumerator SlowAndExplain()
    {
        _isRunning = true;
        Time.timeScale = slowScale;

        // Stap 1
        TutorialManager.Instance.Request(new TutorialStep
        {
            id = "attack_prompt", message = "Druk Linker Muisknop om aan te vallen",
            keyIcons = new[] { "LMB" }, autoDismiss = false
        });
        yield return new WaitForSecondsRealtime(slowDuration);
        TutorialManager.Instance.Dismiss("attack_prompt");
        yield return new WaitForSecondsRealtime(0.3f);

        // Stap 2
        TutorialManager.Instance.Request(new TutorialStep
        {
            id = "lockon_prompt", message = "Druk Tab om te locken op een vijand",
            keyIcons = new[] { "TAB" }, autoDismiss = false
        });
        yield return new WaitForSecondsRealtime(slowDuration);
        TutorialManager.Instance.Dismiss("lockon_prompt");
        yield return new WaitForSecondsRealtime(0.3f);

        // Stap 3
        TutorialManager.Instance.Request(new TutorialStep
        {
            id = "parry_prompt", message = "Vijand valt aan — druk RMB om te parryen",
            keyIcons = new[] { "RMB" }, autoDismiss = false
        });
        yield return new WaitForSecondsRealtime(slowDuration);

        Time.timeScale = 1f;
        TutorialManager.Instance.Dismiss("parry_prompt");
        _isRunning = false;
    }
}