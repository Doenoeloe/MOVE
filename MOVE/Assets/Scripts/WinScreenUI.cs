using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinScreenUI : MonoBehaviour
{
    [Header("Referenties")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button menuButton;

    [Header("Instellingen")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private float _elapsedGameTime;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        retryButton.onClick.AddListener(Retry);
        menuButton.onClick.AddListener(ReturnToMenu);
    }

    public void Show(float elapsedTime)
    {
        _elapsedGameTime = elapsedTime;
        timeText.text = FormatTime(elapsedTime);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}