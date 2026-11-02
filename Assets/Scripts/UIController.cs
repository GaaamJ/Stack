using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text startHint;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private float gameOverDelay = 0.8f;
    [SerializeField] private float punchScale = 1.25f;
    [SerializeField] private float punchTime = 0.15f;
    [SerializeField] private int comboShowFrom = 2;

    private void OnEnable()
    {
        gameManager.StateChanged += OnStateChanged;
        gameManager.ScoreChanged += OnScoreChanged;
        gameManager.ComboChanged += OnComboChanged;
        restartButton.onClick.AddListener(gameManager.Restart);
    }

    private void OnDisable()
    {
        gameManager.StateChanged -= OnStateChanged;
        gameManager.ScoreChanged -= OnScoreChanged;
        gameManager.ComboChanged -= OnComboChanged;
        restartButton.onClick.RemoveListener(gameManager.Restart);
    }

    private void OnStateChanged(GameState state)
    {
        startHint.gameObject.SetActive(state == GameState.Ready);

        StopAllCoroutines();
        gameOverPanel.SetActive(false);
        scoreText.gameObject.SetActive(true);
        comboText.gameObject.SetActive(true);

        if (state == GameState.GameOver)
            StartCoroutine(ShowGameOver());
    }

    private void OnScoreChanged(int score)
    {
        scoreText.text = score.ToString();

        if (score > 0)
            StartCoroutine(Punch(scoreText.rectTransform));
    }

    private void OnComboChanged(int combo)
    {
        comboText.text = combo >= comboShowFrom ? $"PERFECT x{combo}" : string.Empty;
    }

    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(gameOverDelay);
        resultText.text = $"SCORE {gameManager.Score}\nBEST {gameManager.BestScore}";
        scoreText.gameObject.SetActive(false);
        comboText.gameObject.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    private IEnumerator Punch(RectTransform target)
    {
        for (float t = 0f; t < punchTime; t += Time.deltaTime)
        {
            target.localScale = Vector3.one * Mathf.Lerp(punchScale, 1f, t / punchTime);
            yield return null;
        }
        target.localScale = Vector3.one;
    }
}
