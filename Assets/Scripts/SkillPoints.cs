using UnityEngine;

public class SkillPoints : MonoBehaviour
{
    public int Points { get; private set; }

    private void OnEnable()
    {
        Debug.Log("[SkillPoints] Subscribing to EnemyKilled");
        GameEvents.OnEnemyKilled += Add;
    }

    private void OnDisable()
    {
        GameEvents.OnEnemyKilled -= Add;
    }

    private void Add(int amount)
    {
        Points += Mathf.Max(0, amount);
        Debug.Log($"[SkillPoints] +{amount} ? {Points}");
    }
}
