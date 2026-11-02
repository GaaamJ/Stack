using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Ready,
    Playing,
    GameOver,
}

public class GameManager : MonoBehaviour
{
    private const string BestScoreKey = "BestScore";

    [SerializeField] private StackController stack;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private ThemeController theme;
    [SerializeField] private FeedbackFX feedback;

    public event Action<GameState> StateChanged;
    public event Action<int> ScoreChanged;
    public event Action<int> ComboChanged;

    public GameState State { get; private set; }
    public int Score { get; private set; }
    public int Combo { get; private set; }
    public int BestScore { get; private set; }

    private void Start()
    {
        BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        SetState(GameState.Ready);
    }

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            HandleTap();
    }

    private void HandleTap()
    {
        switch (State)
        {
            case GameState.Ready:
                SetState(GameState.Playing);
                stack.Begin();
                break;

            case GameState.Playing:
                OnPlaced(stack.Place());
                break;
        }
    }

    private void OnPlaced(PlaceResult result)
    {
        if (result == PlaceResult.Miss)
        {
            EndGame();
            return;
        }

        Score++;
        Combo = result == PlaceResult.Perfect ? Combo + 1 : 0;
        ScoreChanged?.Invoke(Score);
        ComboChanged?.Invoke(Combo);
        cameraFollow.SetTarget(stack.TopHeight);
        theme.Apply(Score);
        feedback.Play(result, Combo, stack.Top);
    }

    private void EndGame()
    {
        if (Score > BestScore)
        {
            BestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, BestScore);
            PlayerPrefs.Save();
        }

        cameraFollow.ShowWholeTower(stack.TopHeight);
        SetState(GameState.GameOver);
    }

    public void Restart()
    {
        stack.ResetStack();
        cameraFollow.ResetView();
        theme.ResetTheme();

        Score = 0;
        Combo = 0;
        ScoreChanged?.Invoke(Score);
        ComboChanged?.Invoke(Combo);
        SetState(GameState.Ready);
    }

    private void SetState(GameState state)
    {
        State = state;
        StateChanged?.Invoke(state);
    }
}
