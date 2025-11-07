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

    public void Add(int amount)
{
    Points += amount;
    Points = Mathf.Max(0, Points);
    Debug.Log($"[SkillPoints] {amount:+#;-#;0} ? {Points}");
}

}
