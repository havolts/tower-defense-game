using UnityEngine;

public class TickSystem : MonoBehaviour
{
    public float tickInterval = 1f;
    private float tickTimer = 0f;

    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            Tick();
        }
    }

    void Tick()
    {
        // Decision logic
        Debug.Log("Tick at " + Time.time);
    }
}
