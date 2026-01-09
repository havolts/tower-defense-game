using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameController : MonoBehaviour
{

    public static GameController Instance { get; private set; }

    public List<GameObject> friendlyUnits = new List<GameObject>();
    public List<GameObject> enemyUnits = new List<GameObject>();

    public GameObject panel;
    public GameObject endGame;
    public CameraController cam;

    public GameObject fortress;

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

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

    public void Update()
    {
        if(fortress == null) EndGame();
    }

    void EndGame()
    {
        panel.SetActive(false);
        GameController.Instance.StopAllUnits();
        cam.frozen = true;
        endGame.SetActive(true);

    }

    void OnUnitDied(GameObject unit, List<GameObject> unitList)
    {
        unitList.Remove(unit);
    }

    public void StopAllUnits()
    {
        // Stop friendly units
        foreach (GameObject unit in friendlyUnits)
        {
            NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            unit.GetComponent<FriendlyUnit>().frozen = true;
        }

        // Stop enemy units
        foreach (GameObject unit in enemyUnits)
        {
            NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
            if (agent != null) agent.enabled = false;
            unit.GetComponent<EnemyUnit>().frozen = true;
        }
        
    }
}
