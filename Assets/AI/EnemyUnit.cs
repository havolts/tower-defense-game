using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnit : MonoBehaviour
{
    public UnitStats unitStats;
    public float influenceValue = 1.0f;
    public float influenceRange; // world units (metres)
    public float proximityValue = 5.0f;
    public float proximityRange = 2.0f;
    public int skillPointReward = 1;
    public float tickInterval = 1f;
    Transform fortress;

    // Private components
    private NavMeshAgent agent;
    private Health health;

    // Working maps
    private WorkingMap workingMap;
    private WorkingMap gizmoMap; // For debugging only - To be deleted.

    // State tracking
    private float lastAttackTime;
    private float nextAttackTime;
    private float tickTimer = 0f;
    private Order currentOrder = null;
    private Vector2 currentPosition;

    // Pathfinding
    private List<Cell> path = new List<Cell>();
    private int currentIndex = 0;

    void Start()
    {
        GameObject fortressObj = GameObject.Find("Fortress");
            if (fortressObj != null) fortress = fortressObj.transform;
        influenceValue = 1.0f;
        influenceRange = unitStats.attackRange;
        proximityRange = 2.0f;
        proximityValue = 5.0f;
        workingMap = new WorkingMap((4*(int)influenceRange)+1);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = 0f;
        health = GetComponent<Health>();
        health.Died += OnDied;
        currentPosition = new Vector2(this.transform.position.x, this.transform.position.z);
        UpdateTarget();
    }

    void OnDied()
    {
        SkillPoints.Instance.Add(skillPointReward);
    }

    void Update()
    {
        currentPosition = new Vector2(this.transform.position.x, this.transform.position.z);
        FollowPath();
        AttackTarget();
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            Tick();
        }
        if(currentOrder == null || currentOrder.targetTransform == null) return;
        RotateTowards(currentOrder.targetTransform.position);
    }

    void Tick()
    {
        //Debug.Log("Tick at " + Time.time);
        UpdateTarget();
        GetPathToTarget();
    }

    void AttackTarget()
    {
        if (currentOrder == null) {return;}
        if(currentOrder.targetTransform == null) {return;}
        if (Vector3.Distance(currentOrder.targetTransform.position, this.transform.position) <= unitStats.attackRange && Time.time >= lastAttackTime + unitStats.attackCooldown)
        {
            Health health = currentOrder.targetTransform.GetComponent<Health>();
            if (health != null) health.TakeDamage(unitStats.attackDamage);
            lastAttackTime = Time.time;
        }
    }

    WorkingMap GetOwnProximity()
    {
        Vector2Int ownCell = SafetyMap.Instance.ConvertWorldPositionToCellIndex(currentPosition);
        int cellRange = Mathf.CeilToInt(proximityRange / SafetyMap.Instance.cellSize);
        WorkingMap proximityMap = new WorkingMap(cellRange*2);
        proximityMap.startIndex = new Vector2Int(ownCell.x - cellRange, ownCell.y - cellRange);
        proximityMap.currentDiameter = cellRange * 2;

        for (int y = -cellRange; y < cellRange; y++)
        {
            for (int x = -cellRange; x < cellRange; x++)
            {

                int currentX = ownCell.x + x; // offsets the index to the actual current cell in safetymap - local -> global
                int currentY = ownCell.y + y;

                if (currentX < 0 || currentX >= SafetyMap.Instance.gridSizeX || currentY < 0 || currentY >= SafetyMap.Instance.gridSizeZ) continue; // ensures that the cell it is currently checking is within safetymap grid.

                Vector2 currentCellPosition = SafetyMap.Instance.ConvertCellIndexToWorldPosition(new Vector2Int(currentX, currentY)); // Gets real position of the current cell
                Vector2 enemyCellPosition = SafetyMap.Instance.ConvertCellIndexToWorldPosition(ownCell); // Gets the real position of the enemy's cell (is not same as currentPosition)
                float distance = Vector2.Distance(enemyCellPosition, currentCellPosition); // Getting the distance between these two so that we can scale influence accordingly

                if (distance <= proximityRange) // making sure that the cell we are looking at isn't too far
                {
                    float normalizedDistance = distance / proximityRange; // Normalises it
                    float proximity = Mathf.Max(0f, proximityValue * (1f - normalizedDistance * normalizedDistance)); // Quadratic falloff
                    // need to add the proximity into a workingmap then use that to get rid of the units own proximity in position calculation to stop constant changing position.
                    int mapX = x + cellRange;
                    int mapY = y + cellRange;
                    proximityMap.SetCell(mapX, mapY, proximity);
                }

            }
        }
        return proximityMap;
    }

    void GetPathToTarget()
    {
        if (currentOrder == null) {return;}
        if(currentOrder.targetTransform == null) return;
        Vector2 targetPosition = new Vector2(currentOrder.targetTransform.position.x, currentOrder.targetTransform.position.z);
        Vector2Int targetIndex = SafetyMap.Instance.ConvertWorldPositionToCellIndex(targetPosition);
        Cell targetCell = SafetyMap.Instance.GetCell(targetIndex);
        int radius = Mathf.FloorToInt(unitStats.attackRange); // NTS: Will need to be changed if cell size is changed
        WorkingMap enemyProximity = new WorkingMap(radius*2+1);
        enemyProximity.Fill(targetIndex, radius, CellData.enemyProximity);
        enemyProximity.Subtract(GetOwnProximity());

        workingMap.Fill(targetIndex, radius, CellData.friendlyInfluence);
        workingMap.Add(enemyProximity);
        workingMap.Inverse();
        if(targetCell.friendlyInfluence == 0.0f) return; //making sure that the cell actually has a friendly presence.

        Vector2Int positionIndex = workingMap.GetHighestIndex(currentPosition);
        if(positionIndex == new Vector2Int(-1,-1)) return;
        Cell positionCell = SafetyMap.Instance.GetCell(positionIndex);
        Vector2 centreCellPosition = SafetyMap.Instance.ConvertCellIndexToWorldPosition(positionIndex);

        path = PathingSystem.Instance.FindPath(currentPosition, centreCellPosition);
    }

    // All cells are exactly at x.5,y.5 which means the unit stays outside of attack range
    // This code gets the position where the cell's centre and the attack range meet and tells the unit to go there.
    /*Vector2 direction = (centreCellPosition - targetPosition).normalized;
    Vector2 BC = -direction * unitStats.attackRange;
    Vector2 C = targetPosition + BC;*/

    void UpdateTarget()
    {
        Transform closest = GetComponent<Vision>().GetClosestTarget();
        if(closest == null) closest = fortress;
        //Debug.Log(closest.position);
        if(currentOrder != null)
        {
            if (currentOrder.targetTransform.Equals(closest)) {return; }
            if(currentOrder.orderType == OrderType.Move) return;
        }
        currentOrder = new Order(closest, false);
        RotateTowards(closest.position);
    }

    void UpdatePath()
    {
        if(currentOrder.orderType == OrderType.Move) return;
        path = PathingSystem.Instance.FindPath(this.transform.position, currentOrder.targetTransform.position);
    }

    void FollowPath()
    {
        if (path == null || path.Count == 0) return;

        if (currentIndex < 0) currentIndex = 0;

        if (currentIndex >= path.Count)
        {
            path.Clear();
            currentIndex = 0;
            return;
        }

        Vector2 currentPos = SafetyMap.Instance.ConvertVector3ToVector2(transform.position);
        Vector2 targetPos = path[currentIndex].position;

        if (Vector2.Distance(currentPos, targetPos) <= 0.1f)
        {
            currentIndex++;
            return;
        }

        MoveTo(targetPos);
    }

    void MoveTo(Vector3 position)
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 1.0f, NavMesh.AllAreas)) agent.SetDestination(hit.position); //Checking if position is movable
        else currentIndex++; //else try the next position
    }


    void MoveTo(Vector2 position)
    {
        MoveTo(new Vector3(position.x, 0f, position.y));
    }

    void RotateTowards(Vector3 position)
    {
        Vector3 directionToTarget = (position - transform.position).normalized;
        directionToTarget.y = 0;
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            transform.forward = Vector3.Slerp(transform.forward, directionToTarget, Time.deltaTime * 10f);
        }
    }


    public bool drawGizmos = false;

    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (workingMap == null) return;

        WorkingMap gizmoMap = workingMap;
        int currentDiameter = gizmoMap.currentDiameter;
        //gizmoMap.Normalise();

        for (int i = 0; i < currentDiameter; i++)
        {
            for (int j = 0; j < currentDiameter; j++)
            {
                float value = gizmoMap.GetCell(j, i); // normalized 0-1
                Color cellColor = Color.Lerp(Color.white, Color.red, value);
                Gizmos.color = cellColor;
                Vector2 cellPosition = SafetyMap.Instance.ConvertCellIndexToWorldPosition(new Vector2Int(gizmoMap.startIndex.x + j, gizmoMap.startIndex.y + i));
                Vector3 cubePosition = new Vector3(cellPosition.x, 0.1f, cellPosition.y);

                #if UNITY_EDITOR
                    UnityEditor.Handles.Label(cubePosition + Vector3.up * 0.5f, value.ToString("0.00"));
                #endif

                Gizmos.DrawCube(cubePosition, new Vector3(1f, 0.05f, 1f));
            }
        }
    }


}
