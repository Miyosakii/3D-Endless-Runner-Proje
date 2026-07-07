using UnityEngine;
using System;

public static class GameEvents
{
    // Collectible toplandýðýnda
    public static event Action<int> OnCollectibleCollected;
    public static void TriggerCollectibleCollected(int value)
    {
        OnCollectibleCollected?.Invoke(value);
    }

    // Player hasar aldýðýnda (güncel can ile)
    public static event Action<int> OnPlayerDamaged;
    public static void TriggerPlayerDamaged(int currentHealth)
    {
        OnPlayerDamaged?.Invoke(currentHealth);
    }

    // Player öldüðünde
    public static event Action OnPlayerDeath;
    public static void TriggerPlayerDeath()
    {
        OnPlayerDeath?.Invoke();
    }

    // Oyun durumu deðiþtiðinde (isteðe baðlý)
    public static event Action<GameState> OnGameStateChanged;
    public static void TriggerGameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }
}

// Oyun durumlarý
public enum GameState
{
    WaitingToStart,
    Playing,
    Paused,
    GameOver
}