using System.Collections;
using UnityEngine;

public class KillSpell : MonoBehaviour
{
    public string friendlyFortressName = "Fortress";
    public string enemyFortressName = "DarkWizardFortress";
    public float flightDuration = 2f;
    public float arcHeight = 5f;
    public GameObject explosionPrefab;

    private Transform fortress;
    private Transform enemyFortress;
    private Vector3 startPos;
    private Vector3 endPos;
    private float elapsed = 0f;
    private bool flying = false;

    void Start()
    {
        GameObject fObj = GameObject.Find(friendlyFortressName);
        if (fObj != null) fortress = fObj.transform;

        GameObject eObj = GameObject.Find(enemyFortressName);
        if (eObj != null) enemyFortress = eObj.transform;

        if (fortress == null || enemyFortress == null)
        {
            Debug.LogError("Fortress or enemy fortress not found!");
            return;
        }

        startPos = fortress.position;
        endPos = enemyFortress.position;
        transform.position = startPos;
        flying = true;
    }

    void Update()
    {
        if (!flying) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / flightDuration);

        Vector3 midPoint = Vector3.Lerp(startPos, endPos, t);
        float heightOffset = 4 * arcHeight * t * (1 - t);
        transform.position = new Vector3(midPoint.x, midPoint.y + heightOffset, midPoint.z);

        if (t >= 1f)
        {
            flying = false;
            OnHitTarget();
        }
    }

    void OnHitTarget()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, enemyFortress.position, Quaternion.identity);
        }

        Debug.Log("KillSpell hit enemy fortress!");
        Destroy(gameObject);
    }
}
