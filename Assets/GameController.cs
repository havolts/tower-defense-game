using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<GameObject> friendlyUnits = new List<GameObject>();
    public List<GameObject> enemyUnits = new List<GameObject>();

    void Start()
    {
        foreach (FriendlyUnit friendlyUnit in GetComponentsInChildren<FriendlyUnit>())
        {
            friendlyUnits.Add(friendlyUnit.gameObject);
            Health health = friendlyUnit.GetComponent<Health>();
            if (health != null)
            {
                health.Died += () => OnUnitDied(friendlyUnit.gameObject, friendlyUnits);
            }
        }

        foreach (EnemyUnit enemyUnit in GetComponentsInChildren<EnemyUnit>())
        {
            enemyUnits.Add(enemyUnit.gameObject);

            Health health = enemyUnit.GetComponent<Health>();
            if (health != null)
            {
                health.Died += () => OnUnitDied(enemyUnit.gameObject, enemyUnits);
            }
        }
    }

    void OnUnitDied(GameObject unit, List<GameObject> unitList)
        {
            unitList.Remove(unit);
            Debug.Log($"Unit {unit.name} removed from list. Remaining: {unitList.Count}");
        }

}
