using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<int> OnEnemyKilled;

    public static void EnemyKilled(int reward)
    {
        Debug.Log($"[GameEvents] EnemyKilled({reward})");
        OnEnemyKilled?.Invoke(reward);
    }
}
