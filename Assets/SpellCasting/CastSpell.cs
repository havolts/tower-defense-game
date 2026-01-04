using UnityEngine;

public class CastSpell : MonoBehaviour
{
    public SpellType type;
    private bool waitingForClick = false;

    public GameObject meteorPrefab;
    public GameObject dispersePrefab;
    public GameObject suspendPrefab;
    public GameObject stonePrefab;

    private float lastMeteorCastTime = -100f;
    private float lastDisperseCastTime = -100f;
    private float lastSuspendCastTime = -100f;
    private float lastStoneCastTime = -100f;

    public void OnButtonPressed()
    {
        if (!IsCooldownOver(type))
        {
            Debug.Log(type + " is still on cooldown!");
            return;
        }

        waitingForClick = true;
        Debug.Log("Waiting for target click for: " + type);
    }

    void Update()
    {
        if (!waitingForClick) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (IsCooldownOver(type))
                {
                    CastAtPosition(hit.point);
                    UpdateCooldown(type);
                }
                waitingForClick = false;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            waitingForClick = false;
        }
    }

    bool IsCooldownOver(SpellType spellType)
    {
        float currentTime = Time.time;

        switch (spellType)
        {
            case SpellType.Meteor:
                return currentTime >= lastMeteorCastTime + SpellDatabase.Instance.meteorSpell.cooldown;
            case SpellType.Disperse:
                return currentTime >= lastDisperseCastTime + SpellDatabase.Instance.disperseSpell.cooldown;
            case SpellType.Suspend:
                return currentTime >= lastSuspendCastTime + SpellDatabase.Instance.suspendSpell.cooldown;
            case SpellType.Stone:
                return currentTime >= lastStoneCastTime + SpellDatabase.Instance.stoneSpell.cooldown;
            default:
                return true;
        }
    }

    void UpdateCooldown(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Meteor: lastMeteorCastTime = Time.time; break;
            case SpellType.Disperse: lastDisperseCastTime = Time.time; break;
            case SpellType.Suspend: lastSuspendCastTime = Time.time; break;
            case SpellType.Stone: lastStoneCastTime = Time.time; break;
        }
    }

    void CastAtPosition(Vector3 position)
    {
        if (type == SpellType.Meteor) CastMeteor(position);
        else if (type == SpellType.Disperse) CastDisperse(position);
        else if (type == SpellType.Suspend) CastSuspend(position);
        else if (type == SpellType.Stone) CastStone(position);
    }

    void CastMeteor(Vector3 position)
    {
        GameObject bolt = Instantiate(meteorPrefab, new Vector3(-50f, 30, 0f), Quaternion.identity);
        Meteor meteor = bolt.GetComponent<Meteor>();
        float damage = SpellDatabase.Instance.meteorSpell.damage;
        float radius = SpellDatabase.Instance.meteorSpell.radius;
        meteor.LaunchAtPosition(position, damage, radius);
    }

    void CastDisperse(Vector3 position)
    {
        GameObject bolt = Instantiate(dispersePrefab, new Vector3(-50f, 30, 0f), Quaternion.identity);
        Disperse disperse = bolt.GetComponent<Disperse>();
        float radius = SpellDatabase.Instance.disperseSpell.radius;
        disperse.LaunchAtPosition(position, radius);
    }

    void CastSuspend(Vector3 position)
    {
        Quaternion rotation = Quaternion.Euler(90f, 0f, 0f);
        position.y += 0.5f;
        Instantiate(suspendPrefab, position, rotation);
    }

    void CastStone(Vector3 position)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
        Instantiate(stonePrefab, position, rotation);
    }
}
