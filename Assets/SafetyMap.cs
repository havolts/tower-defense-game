using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafetyMap : MonoBehaviour
{
    Cell[,] cellmap;
    public GameController controller;

    int cellSize = 1;
    int numberOfCellsX;
    int numberOfCellsZ;
    int terrainSizeX;
    int terrainSizeZ;
    void Start()
    {
        terrainSizeX = (int)this.GetComponent<MeshRenderer>().bounds.size.x;
        terrainSizeZ = (int)this.GetComponent<MeshRenderer>().bounds.size.z;

        numberOfCellsX = terrainSizeX / cellSize;
        numberOfCellsZ = terrainSizeZ / cellSize;

        cellmap = new Cell[numberOfCellsZ, numberOfCellsX];
        for (int row = 0; row < numberOfCellsZ; row++)
        {
            for (int column = 0; column < numberOfCellsX; column++)
            {
                cellmap[row, column] = new Cell(); // Initial allocation
            }
        }
    }

    void FixedUpdate()
    {
        ClearCellMap();
        foreach(GameObject unit in controller.enemyUnits)
        {
            Vector2 unitPosition = new Vector2(unit.transform.position.x, unit.transform.position.z);
            Vector2 unitForwardDirection = new Vector2(unit.transform.forward.x, unit.transform.forward.z);
            Vector2 unitRightDirection = new Vector2(unit.transform.right.x, unit.transform.right.z);
            Vector2 unitForwardRightDirection = unitForwardDirection + unitRightDirection;
            Vector2 unitForwardLeftDirection = unitForwardDirection - unitRightDirection;
            float unitForwardRightAngle = Vector2.SignedAngle(unitForwardRightDirection, Vector2.up);
            float unitForwardLeftAngle = Vector2.SignedAngle(unitForwardLeftDirection, Vector2.up);
            float unitForwardAngle = Vector2.SignedAngle(unitForwardDirection, Vector2.up);

            float baseSafety = 1.0f;

            ModifyCellSafety(ConvertWorldPositionToCellIndex(unitPosition), baseSafety);
            ModifyCellSafety(ConvertSectorToCellIndex(ConvertAngleToSector(unitForwardAngle), unitPosition), baseSafety);
            ModifyCellSafety(ConvertSectorToCellIndex(ConvertAngleToSector(unitForwardRightAngle), unitPosition), baseSafety);
            ModifyCellSafety(ConvertSectorToCellIndex(ConvertAngleToSector(unitForwardLeftAngle), unitPosition), baseSafety);
            ModifySafetyAroundUnit(unitPosition, baseSafety*0.5f);
            ModifyCellSafetyOnRing(unitPosition, 2, baseSafety*0.25f);
            ModifyCellSafetyOnRing(unitPosition, 3, baseSafety*0.1f);
        }
        foreach(GameObject unit in controller.friendlyUnits)
        {
            Vector2 unitPosition = new Vector2(unit.transform.position.x, unit.transform.position.z);
            Vector2 unitForwardDirection = new Vector2(unit.transform.forward.x, unit.transform.forward.z);
            Vector2 unitRightDirection = new Vector2(unit.transform.right.x, unit.transform.right.z);
            Vector2 unitForwardRightDirection = unitForwardDirection + unitRightDirection;
            Vector2 unitForwardLeftDirection = unitForwardDirection - unitRightDirection;
            float unitForwardRightAngle = Vector2.SignedAngle(unitForwardRightDirection, Vector2.up);
            float unitForwardLeftAngle = Vector2.SignedAngle(unitForwardLeftDirection, Vector2.up);
            float unitForwardAngle = Vector2.SignedAngle(unitForwardDirection, Vector2.up);

            float baseSafety = -1.0f;

            ModifyCellSafety(ConvertWorldPositionToCellIndex(unitPosition), baseSafety);
            ModifyCellSafety(ConvertSectorToCellIndex(ConvertAngleToSector(unitForwardAngle), unitPosition), baseSafety);
            ModifyCellSafety(ConvertSectorToCellIndex(ConvertAngleToSector(unitForwardRightAngle), unitPosition), baseSafety);
            ModifyCellSafety(ConvertSectorToCellIndex(ConvertAngleToSector(unitForwardLeftAngle), unitPosition), baseSafety);
            ModifySafetyAroundUnit(unitPosition, baseSafety*0.5f);
            ModifyCellSafetyOnRing(unitPosition, 2, baseSafety*0.25f);
            ModifyCellSafetyOnRing(unitPosition, 3, baseSafety*0.1f);
        }
    }

    int ConvertAngleToSector(float angle) // returns a 'sector' - think cardinal direction, 0 is north, 7 is northwest
    {
        if(angle < 0) angle += 360.0f; // converts a signed angle to a 360 degrees range angle

        return Mathf.FloorToInt(((angle+22.5f) % 360.0f) / 45.0f); // adding 22.5 as forward is range of -22.5 to 22.5. This gets the 'sector' of the direction.
    }

    Vector2Int ConvertSectorToCellIndex(int sector, Vector2 unitPosition)
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

    void ModifyCellSafety(Vector2Int cellIndex, float safetyModifier)
    {
        // Add bounds check
        if (cellIndex.x >= 0 && cellIndex.x < numberOfCellsX && cellIndex.y >= 0 && cellIndex.y < numberOfCellsZ)
        {
            cellmap[cellIndex.y, cellIndex.x].safety = Mathf.Clamp(cellmap[cellIndex.y, cellIndex.x].safety + safetyModifier, -1.0f, 1.0f);
        }
    }

    void ModifySafetyAroundUnit(Vector2 unitPosition, float safetyModifier)
    {
        Vector2Int cellIndex = ConvertWorldPositionToCellIndex(unitPosition);
        for (int i = cellIndex.y -1; i <= cellIndex.y + 1; i++)
        {
            for (int j = cellIndex.x -1; j <= cellIndex.x + 1; j++)
            {
                if (i >= 0 && i < numberOfCellsZ && j >= 0 && j < numberOfCellsX)
                {
                    ModifyCellSafety(new Vector2Int(j, i), safetyModifier);
                }
            }
        }
    }

    void ModifyCellSafetyOnRing(Vector2 unitPosition, int distance, float safetyModifier) // THIS METHOD IS ENTIRELY AI CODE - GEMINI
    {
        // 1. Get the center cell index
        Vector2Int centerCell = ConvertWorldPositionToCellIndex(unitPosition);
        int centerX = centerCell.x;
        int centerY = centerCell.y;

        // The bounds of the square to check are defined by the distance 'D'
        int minX = centerX - distance;
        int maxX = centerX + distance;
        int minY = centerY - distance;
        int maxY = centerY + distance;

        // Loop over the full square region
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                // 2. Calculate the distance from the center cell (Manhattan distance, essentially)
                int offsetX = Mathf.Abs(x - centerX);
                int offsetY = Mathf.Abs(y - centerY);

                // 3. Conditional Check: Only modify cells exactly on the ring
                // The max of the two offsets must equal the desired distance 'D'.
                if (Mathf.Max(offsetX, offsetY) == distance)
                {
                    // 4. Apply safety only to the cells on the perimeter/ring

                    // Add your standard bounds check here
                    if (x >= 0 && x < numberOfCellsX && y >= 0 && y < numberOfCellsZ)
                    {
                        // Assuming you have a simplified ModifyCellSafety that handles clamping
                        ModifyCellSafety(new Vector2Int(x, y), safetyModifier);
                    }
                }
            }
        }
    }

    void ClearCellMap()
    {
        for (int row = 0; row < numberOfCellsZ; row++)
        {
            for (int column = 0; column < numberOfCellsX; column++)
            {
                cellmap[row, column].safety = 0.0f;
            }
        }
    }

    Vector2Int ConvertWorldPositionToCellIndex(Vector2 worldPosition)
    {
        float positionX = (worldPosition.x) + (terrainSizeX / 2);
        int cellIndexX = Mathf.FloorToInt(positionX / cellSize);
        float positionZ = (worldPosition.y) + (terrainSizeZ / 2);
        int cellIndexY = Mathf.FloorToInt(positionZ / cellSize);
        return new Vector2Int(cellIndexX, cellIndexY);
    }

    Vector2 ConvertCellIndexToWorldPosition(Vector2Int cellIndex)
    {
        float positionX = cellIndex.x * cellSize;
        positionX = positionX - (terrainSizeX / 2) + (cellSize / 2.0f);
        float positionZ = cellIndex.y * cellSize;
        positionZ = positionZ - (terrainSizeZ / 2) + (cellSize / 2.0f);
        return new Vector2(positionX, positionZ);
    }

    void OnDrawGizmos()
    {
        if (cellmap == null) return;

        for (int row = 0; row < numberOfCellsZ; row++)
        {
            for (int column = 0; column < numberOfCellsX; column++)
            {
                Vector2 cellPosition = ConvertCellIndexToWorldPosition(new Vector2Int(column, row));
                Vector3 cubePosition = new Vector3(cellPosition.x, 1.0f, cellPosition.y);
                Gizmos.color = new Color(0,0,0,0);
                float safety = cellmap[row, column].safety;
                if (safety > 0.0f)
                {
                    Gizmos.color = new Color(0, 0, safety, 1);
                }
                if (safety < 0.0f)
                {
                    safety = Mathf.Abs(cellmap[row, column].safety);
                    Gizmos.color = new Color(safety, 0, 0, 1);
                }
                Gizmos.DrawCube(cubePosition, new Vector3(cellSize,cellSize,cellSize));
            }
        }
    }
}
