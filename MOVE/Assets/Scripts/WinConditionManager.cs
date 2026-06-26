using UnityEngine;

public class WinConditionManager : MonoBehaviour
{
    [SerializeField] private WinScreenUI winScreenUI;

    private float _startTime;

    private void Start() => _startTime = Time.time;

    private void OnEnable()  => HealthComponent.OnBossDefeated += HandleWin;
    private void OnDisable() => HealthComponent.OnBossDefeated -= HandleWin;

    private void HandleWin()
    {
        float elapsed = Time.time - _startTime;
        Time.timeScale = 0f;
        winScreenUI.Show(elapsed);
    }
}