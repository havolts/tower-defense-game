using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafetyMap : MonoBehaviour
{
    public Cell[,] grid;
    public GameController controller;

    public int cellSize = 1;
    public int gridSizeX;
    public int gridSizeZ;
    int terrainSizeX;
    int terrainSizeZ;

    public static SafetyMap Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void SetupGrid()
    {
        terrainSizeX = (int)this.GetComponent<MeshRenderer>().bounds.size.x;
        terrainSizeZ = (int)this.GetComponent<MeshRenderer>().bounds.size.z;

        gridSizeX = terrainSizeX / cellSize;
        gridSizeZ = terrainSizeZ / cellSize;

        grid = new Cell[gridSizeZ, gridSizeX];
        for (int y = 0; y < gridSizeZ; y++) // vertical component - i.e. the rows (each array in 2d grid)
        {
            for (int x = 0; x < gridSizeX; x++) // horizontal - i.e. the columns (each element in array)
            {
                SetCell(x, y, new Cell(x,y));
            }
        }
    }

    void SetCell(int x, int y, Cell cell)
    {
        grid[y, x] = cell;
    }

    public Cell GetCell(int x, int y)
    {
        return grid[y, x];
    }

    public Cell GetCell(Vector2Int index)
    {
        return grid[index.y, index.x];
    }

    void Start()
    {
        SetupGrid();
    }

    public Vector2 ConvertVector3ToVector2(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    void FixedUpdate()
    {
        ClearInfluence();
        foreach(GameObject unit in controller.enemyUnits)
        {
            EnemyUnit enemy = unit.GetComponent<EnemyUnit>();
            Vector2 unitPosition = ConvertVector3ToVector2(unit.transform.position);
            // Apply influence for enemies:
            ApplyEnemyInfluence(unitPosition, enemy.influenceValue, enemy.influenceRange);
            ApplyEnemyProximity(unitPosition, enemy.proximityValue, enemy.proximityRange);
        }
        foreach(GameObject unit in controller.friendlyUnits)
        {
            FriendlyUnit friendly = unit.GetComponent<FriendlyUnit>();
            Vector2 unitPosition = ConvertVector3ToVector2(unit.transform.position);
            // Apply influence for enemies:
            ApplyFriendlyInfluence(unitPosition, friendly.influenceValue, friendly.influenceRange);
        }
    }

    void ClearInfluence()
    {
        foreach (Cell cell in grid)
        {
            cell.enemyInfluence = 0f;
            cell.friendlyInfluence = 0f;
            cell.enemyProximity = 0f;
        }
    }

    void ApplyEnemyInfluence(Vector2 enemyPosition, float influenceValue, float influenceRange)
    {
        Vector2Int enemyCell = ConvertWorldPositionToCellIndex(enemyPosition);
        int cellRange = Mathf.CeilToInt(influenceRange / cellSize);

        for (int y = -cellRange; y <= cellRange; y++)
        {
            for (int x = -cellRange; x <= cellRange; x++)
            {

                int currentX = enemyCell.x + x; // offsets the index to the actual current cell in safetymap - local -> global
                int currentY = enemyCell.y + y;

                if (currentX < 0 && currentX >= gridSizeX && currentY < 0 && currentY >= gridSizeZ) continue; // ensures that the cell it is currently checking is within grid.

                Vector2 currentCellPosition = ConvertCellIndexToWorldPosition(new Vector2Int(currentX, currentY)); // Gets real position of the current cell
                Vector2 enemyCellPosition = ConvertCellIndexToWorldPosition(enemyCell); // Gets the real position of the enemy's cell
                float distance = Vector2.Distance(enemyCellPosition, currentCellPosition); // Getting the distance between these two so that we can scale influence accordingly

                if (distance <= influenceRange) // making sure that the cell we are looking at isn't too far
                {
                    float normalizedDistance = distance / influenceRange; // Normalises it
                    float influence = Mathf.Max(0f, influenceValue * (1f - normalizedDistance * normalizedDistance)); // Quadratic falloff

                    GetCell(currentX, currentY).enemyInfluence += influence; // Adds to the cell we are looking at
                }

            }
        }
    }

    void ApplyFriendlyInfluence(Vector2 friendlyPosition, float influenceValue, float influenceRange)
    {
        Vector2Int centerCell = ConvertWorldPositionToCellIndex(friendlyPosition);
        int cellRange = Mathf.CeilToInt(influenceRange / cellSize);

        for (int y = -cellRange; y <= cellRange; y++)
        {
            for (int x = -cellRange; x <= cellRange; x++)
            {

                int targetX = centerCell.x + x;
                int targetY = centerCell.y + y;

                if (targetX >= 0 && targetX < gridSizeX && targetY >= 0 && targetY < gridSizeZ)
                {
                    Vector2 cellWorldPos = ConvertCellIndexToWorldPosition(new Vector2Int(targetX, targetY));
                    Vector2 friendlyCellCenter = ConvertCellIndexToWorldPosition(centerCell);
                    float distance = Vector2.Distance(friendlyCellCenter, cellWorldPos);

                    if (distance <= influenceRange)
                    {
                        float normalizedDistance = distance / influenceRange;

                        // Linear falloff
                        //float influence = Mathf.Max(0f, influenceValue * (1f - normalizedDistance));

                        // OR quadratic falloff
                         float influence = Mathf.Max(0f, influenceValue * (1f - normalizedDistance * normalizedDistance));

                        GetCell(targetX, targetY).friendlyInfluence += influence;
                    }
                }
            }
        }
    }

    void ApplyEnemyProximity(Vector2 friendlyPosition, float proximityValue, float proximityRange)
    {
        Vector2Int centerCell = ConvertWorldPositionToCellIndex(friendlyPosition);
        int cellRange = Mathf.CeilToInt(proximityRange / cellSize);

        for (int y = -cellRange; y <= cellRange; y++)
        {
            for (int x = -cellRange; x <= cellRange; x++)
            {

                int targetX = centerCell.x + x;
                int targetY = centerCell.y + y;

                if (targetX >= 0 && targetX < gridSizeX && targetY >= 0 && targetY < gridSizeZ)
                {
                    Vector2 cellWorldPos = ConvertCellIndexToWorldPosition(new Vector2Int(targetX, targetY));
                    Vector2 enemyCellCenter = ConvertCellIndexToWorldPosition(centerCell);
                    float distance = Vector2.Distance(enemyCellCenter, cellWorldPos);

                    if (distance <= proximityRange)
                    {
                        float normalizedDistance = distance / proximityRange;

                        // Linear falloff
                        float proximity = Mathf.Max(0f, proximityValue * (1f - normalizedDistance));

                        // OR quadratic falloff
                        //float proximity = Mathf.Max(0f, proximityValue * (1f - normalizedDistance * normalizedDistance));

                        GetCell(targetX, targetY).enemyProximity += proximity;
                    }
                }
            }
        }
    }

    int ConvertAngleToSector(float angle) // returns a 'sector' - think cardinal direction, 0 is north, 7 is northwest
    {
        if(angle < 0) angle += 360.0f; // converts a signed angle to a 360 degrees range angle

        return Mathf.FloorToInt(((angle+22.5f) % 360.0f) / 45.0f); // adding 22.5 as forward is range of -22.5 to 22.5. This gets the 'sector' of the direction.
    }

    Vector2Int ConvertSectorToCellIndex(int sector, Vector2 unitPosition) // maybe use a switch statement?
    {
        Vector2Int unitCell = ConvertWorldPositionToCellIndex(unitPosition);
        if(sector == 0)
        {
            unitCell.y += 1;
        }
        else if (sector == 1)
        {
            unitCell.x += 1;
            unitCell.y += 1;
        }
        else if (sector == 2)
        {
            unitCell.x += 1;
        }
        else if (sector == 3)
        {
            unitCell.x += 1;
            unitCell.y -= 1;
        }
        else if (sector == 4)
        {
            unitCell.y -= 1;
        }
        else if (sector == 5)
        {
            unitCell.x -= 1;
            unitCell.y -= 1;
        }
        else if (sector == 6)
        {
            unitCell.x -= 1;
        }
        else if (sector == 7)
        {
            unitCell.x -= 1;
            unitCell.y += 1;
        }
        return unitCell;
    }


    public Vector2Int ConvertWorldPositionToCellIndex(Vector2 worldPosition)
    {
        float positionX = (worldPosition.x) + (terrainSizeX / 2);
        int cellIndexX = Mathf.FloorToInt(positionX / cellSize);
        float positionZ = (worldPosition.y) + (terrainSizeZ / 2);
        int cellIndexY = Mathf.FloorToInt(positionZ / cellSize);
        return new Vector2Int(cellIndexX, cellIndexY);
    }

    public Vector2 ConvertCellIndexToWorldPosition(Vector2Int cellIndex)
    {
        float positionX = cellIndex.x * cellSize;
        positionX = positionX - (terrainSizeX / 2) + (cellSize / 2.0f);
        float positionZ = cellIndex.y * cellSize;
        positionZ = positionZ - (terrainSizeZ / 2) + (cellSize / 2.0f);
        return new Vector2(positionX, positionZ);
    }

    /*void OnDrawGizmos()
    {
        if (grid == null) return;

        float maxExpectedInfluence = 1.0f;

        for (int row = 0; row < gridSizeZ; row++)
        {
            for (int column = 0; column < gridSizeX; column++)
            {
                float enemyInfluence = grid[row, column].enemyInfluence;
                float friendlyInfluence = grid[row, column].friendlyInfluence;

                Vector2 cellPos = ConvertCellIndexToWorldPosition(new Vector2Int(column, row));
                Vector3 cubePosition = new Vector3(cellPos.x, 0.1f, cellPos.y);

                // Enemy influence
                if (enemyInfluence > 0.01f)
                {
                    float alpha = Mathf.Clamp01(enemyInfluence / maxExpectedInfluence);
                    Gizmos.color = new Color(1, 0, 0, alpha);
                    Gizmos.DrawCube(cubePosition, new Vector3(cellSize, 0.05f, cellSize));

                #if UNITY_EDITOR
                                UnityEditor.Handles.Label(cubePosition + Vector3.up * 0.05f, enemyInfluence.ToString("0.00"));
                #endif
                }

                // Friendly influence
                if (friendlyInfluence > 0.01f)
                {
                    float alpha = Mathf.Clamp01(friendlyInfluence / maxExpectedInfluence);
                    Gizmos.color = new Color(0, 0, 1, alpha);
                    Gizmos.DrawCube(cubePosition, new Vector3(cellSize, 0.05f, cellSize));

                #if UNITY_EDITOR
                                UnityEditor.Handles.Label(cubePosition + Vector3.up * 0.05f, friendlyInfluence.ToString("0.00"));
                #endif
                }

                float enemyProximity = grid[row, column].enemyProximity; // ensure Cell class has this field
                if (enemyProximity > 0.01f)
                {
                    float alpha = Mathf.Clamp01(enemyProximity / maxExpectedInfluence);
                    Gizmos.color = new Color(1, 0.5f, 0, alpha); // orange
                    Gizmos.DrawCube(cubePosition, new Vector3(cellSize, 0.05f, cellSize));
                #if UNITY_EDITOR
                    UnityEditor.Handles.Label(cubePosition + Vector3.up * 0.05f, enemyProximity.ToString("0.00"));
                #endif
                }
            }
        }
        }*/

}
