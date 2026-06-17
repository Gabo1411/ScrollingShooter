using UnityEngine;
using System;

/// <summary>
/// Singleton that manages global game state: score, lives, and game flow.
/// Other scripts subscribe to events to react to state changes.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public bool IsGameOver { get; private set; }

    // Events for UI and other systems to react to state changes
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnGameOver;
    public event Action OnGameRestart;

    private const int StartLives = 3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>Initializes a new game session.</summary>
    public void StartGame()
    {
        Score = 0;
        Lives = StartLives;
        IsGameOver = false;
        OnScoreChanged?.Invoke(Score);
        OnLivesChanged?.Invoke(Lives);
    }

    /// <summary>Adds points to the current score.</summary>
    public void AddScore(int points)
    {
        if (IsGameOver) return;
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    /// <summary>Decrements lives by one. Triggers game over if lives reach zero.</summary>
    public void LoseLife()
    {
        if (IsGameOver) return;
        Lives--;
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
        }
    }

    /// <summary>Cleans up the field and restarts the game.</summary>
    public void RestartGame()
    {
        // Destroy all enemies and bullets in the scene
        foreach (var enemy in FindObjectsOfType<Enemy>())
            Destroy(enemy.gameObject);
        foreach (var bullet in FindObjectsOfType<Bullet>())
            Destroy(bullet.gameObject);

        StartGame();
        OnGameRestart?.Invoke();
    }
}
