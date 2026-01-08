using UnityEngine;

public class WorkingMap
{
    public float[,] grid;
    public Vector2Int startIndex;
    public int fixedDiameter;
    public int currentDiameter;


    public WorkingMap(int inDiameter)
    {
        fixedDiameter = inDiameter;
        grid = new float[fixedDiameter, fixedDiameter];
        Clear();
    }

    public void SetCell(int x, int y, float value)
    {
        grid[y, x] = value;
    }

    public float GetCell(int x, int y)
    {
        return grid[y, x];
    }

    public void Clear()
    {
        for (int y = 0; y < fixedDiameter; y++)
        {
            for (int x = 0; x < fixedDiameter; x++)
            {
                SetCell(x, y, 0.0f);
            }
        }
    }

    public void Set(float value)
    {
        for (int y = 0; y < fixedDiameter; y++)
        {
            for (int x = 0; x < fixedDiameter; x++)
            {
                SetCell(x, y, value);
            }
        }
    }

    public void Add(WorkingMap other)
    {
        Vector2Int startOffset = other.startIndex - startIndex;

        for (int y = 0; y < other.currentDiameter; y++)
        {
            for (int x = 0; x < other.currentDiameter; x++)
            {
                int offsetX = startOffset.x + x;
                int offsetY = startOffset.y + y;

                if (offsetX < 0 || offsetY < 0 || offsetX >= currentDiameter || offsetY >= currentDiameter)
                    continue;

                float sum = GetCell(offsetX, offsetY) + other.GetCell(x, y);
                SetCell(offsetX, offsetY, sum);
            }
        }
    }

    public void Subtract(WorkingMap other)
    {
        Vector2Int startOffset = other.startIndex - startIndex;

        for (int y = 0; y < other.currentDiameter; y++)
        {
            for (int x = 0; x < other.currentDiameter; x++)
            {
                int offsetX = startOffset.x + x;
                int offsetY = startOffset.y + y;

                if (offsetX < 0 || offsetY < 0 || offsetX >= currentDiameter || offsetY >= currentDiameter)
                    continue;

                float sum = GetCell(offsetX, offsetY) - other.GetCell(x, y);
                SetCell(offsetX, offsetY, Mathf.Max(0f, sum));
            }
        }
    }

    public void Normalise()
    {
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < currentDiameter; i++)
        {
            for (int j = 0; j < currentDiameter; j++)
            {
                float value = GetCell(j, i);

                if(value > max) max = value;
                if(value < min) min = value;
            }
        }

        if(max <= min) return;

        for (int i = 0; i < currentDiameter; i++)
        {
            for (int j = 0; j < currentDiameter; j++)
            {
                float value = GetCell(j, i);
                float normalised = (value - min) / (max - min);
                SetCell(j, i, normalised);
            }
        }
    }

    public void Inverse()
    {
        Normalise();
        for (int i = 0; i < currentDiameter; i++)
        {
            for (int j = 0; j < currentDiameter; j++)
            {
                float value = GetCell(j, i);
                float inverse = 1.0f - value;
                SetCell(j, i, inverse);
            }
        }
    }

    public void Fill(Vector2Int centreIndex, int radius, CellData data)
    {
        Clear();

        currentDiameter = radius * 2 + 1;
        startIndex = new Vector2Int(centreIndex.x - radius, centreIndex.y - radius);

        for (int i = 0; i < currentDiameter; i++)
        {
            for (int j = 0; j < currentDiameter; j++)
            {
                if (startIndex.y + i < 0 || startIndex.y + i >= SafetyMap.Instance.gridSizeX)
                {
                    SetCell(j, i, 0f);
                    continue;
                }
                if (startIndex.x + j < 0 || startIndex.x + j >= SafetyMap.Instance.gridSizeZ)
                {
                    SetCell(j, i, 0f);
                    continue;
                }

                SetCell(j, i, GetCellValue(startIndex.x+j, startIndex.y+i, data));
            }
        }
    }

    float GetCellValue(int x, int y, CellData data)
    {
        Cell cell = SafetyMap.Instance.GetCell(x, y);
        switch (data)
        {
            case CellData.friendlyInfluence:
                return cell.friendlyInfluence;

            case CellData.friendlyProximity:
                return cell.friendlyProximity;

            case CellData.enemyInfluence:
                return cell.enemyInfluence;

            case CellData.enemyProximity:
                return cell.enemyProximity;

            default:
                return -1f;
        }
    }

    public Vector2Int GetHighestIndex(Vector2 unitPosition)
    {
        Debug.Log("unit position: " + unitPosition);
        float highest = 0.0f;
        float distance = float.MaxValue;
        Vector2Int highestIndex = new Vector2Int(-1, -1);

        int mapWidth = SafetyMap.Instance.gridSizeX;
        int mapHeight = SafetyMap.Instance.gridSizeZ;

        for (int y = 0; y < currentDiameter; y++)
        {
            int mapY = startIndex.y + y;
            if (mapY < 0 || mapY >= mapHeight) continue;

            for (int x = 0; x < currentDiameter; x++)
            {

                int mapX = startIndex.x + x;
                if (mapX < 0 || mapX >= mapWidth) continue;


                float current = GetCell(x, y);
                if(current > highest)
                {
                    highest = current;
                    highestIndex = new Vector2Int(startIndex.x + x, startIndex.y + y);
                    Vector2 currentCellPosition = SafetyMap.Instance.GetCell(startIndex.x + x, startIndex.y + y).position;
                    Debug.Log("Current Cell Position: " + currentCellPosition);

                    distance = Vector2.Distance(unitPosition, currentCellPosition);
                }
                if(Mathf.Approximately(current,highest))
                {
                    Vector2 currentCellPosition = SafetyMap.Instance.GetCell(startIndex.x + x, startIndex.y + y).position;
                    if(Vector2.Distance(unitPosition, currentCellPosition) < distance)
                    {
                        highest = current;
                        highestIndex = new Vector2Int(startIndex.x + x, startIndex.y + y);
                        distance = Vector2.Distance(unitPosition, currentCellPosition);
                    }
                }
            }
        }

        return highestIndex;
    }

    public void PrintGrid()
    {
        for (int i = 0; i <= currentDiameter; i++)
        {
            string row = "";
            for (int j = 0; j <= currentDiameter; j++)
            {
                row += " | " + GetCell(i, j).ToString("0.000") + " | ";
            }
            Debug.Log(row);
        }
    }
}
