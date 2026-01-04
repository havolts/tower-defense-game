using UnityEngine;

public class SkillPoints : MonoBehaviour
{
    public static SkillPoints Instance { get; private set; }
    public int points { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
        points = 5;
    }

    public void Add(int toAdd)
    {
        points += toAdd;
    }
    public void Subtract(int toSubtract)
    {
        points -= toSubtract;
    }

}
