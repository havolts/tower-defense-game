using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathingSystem : MonoBehaviour
{
    public static PathingSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }

    void Start()
    {
    }

    float Heuristic(Cell start, Cell goal)
    {
        float a = (goal.position.x - start.position.x) * (goal.position.x - start.position.x);
        float b = (goal.position.y - start.position.y) * (goal.position.y - start.position.y);
        return Mathf.Sqrt(a+b);
    }

    List<Vector2Int> GetValidNeighbours(Cell currentCell)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        List<Vector2Int> possibleMoves = new List<Vector2Int>
        {
            new Vector2Int(currentCell.index.x + 1, currentCell.index.y), new Vector2Int(currentCell.index.x - 1, currentCell.index.y),
            new Vector2Int(currentCell.index.x, currentCell.index.y + 1), new Vector2Int(currentCell.index.x, currentCell.index.y - 1),
            new Vector2Int(currentCell.index.x + 1, currentCell.index.y + 1), new Vector2Int(currentCell.index.x - 1, currentCell.index.y - 1),
            new Vector2Int(currentCell.index.x + 1, currentCell.index.y - 1), new Vector2Int(currentCell.index.x - 1, currentCell.index.y + 1)
        };

        foreach(Vector2Int potentialPosition in possibleMoves)
        {
            if (potentialPosition.x < 0 || potentialPosition.x >= SafetyMap.Instance.gridSizeX) continue;
            if (potentialPosition.y < 0 || potentialPosition.y >= SafetyMap.Instance.gridSizeZ) continue;

            neighbours.Add(potentialPosition);
        }
        return neighbours;
    }

    List<Cell> ReconstructPath(Cell goal)
    {
        List<Cell> path = new List<Cell>();
        Cell current = goal;

        while(current != null)
        {
            path.Add(current);
            current = current.parent;
        }
        return path;
    }

    public List<Cell> FindPath(Cell start, Cell goal)
    {
        for (int y = 0; y < SafetyMap.Instance.gridSizeZ; y++)
        {
            for (int x = 0; x < SafetyMap.Instance.gridSizeX; x++)
            {
                Cell cell = SafetyMap.Instance.grid[y, x];
                cell.g = float.MaxValue;
                cell.h = 0.0f;
                cell.f = 0.0f;
                cell.parent = null;
            }
        }
        List<Cell> openList = new List<Cell>(); // Should be changed to a priority queue
        openList.Add(start);

        List<Cell> closedList = new List<Cell>();

        start.g = 0.0f;
        start.h = Heuristic(start, goal);
        start.f = start.g + start.h;

        while(openList.Count != 0)
        {
            Cell current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f < current.f)
                {
                    current = openList[i];
                }
            }

            if(current == goal)
            {
                List<Cell> path = ReconstructPath(goal);
                path.Reverse();
                return path;
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach(Vector2Int neighbourPosition in GetValidNeighbours(current))
            {
                Cell neighbor = SafetyMap.Instance.grid[neighbourPosition.y, neighbourPosition.x];

                if (closedList.Contains(neighbor)) continue;

                float tentativeG = current.g + Heuristic(current, neighbor);

                if (!openList.Contains(neighbor) || tentativeG < neighbor.g)
                {
                    neighbor.g = tentativeG;
                    neighbor.h = Heuristic(neighbor, goal);
                    neighbor.f = neighbor.g + neighbor.h;
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        return new List<Cell>();
    }

    public List<Cell> FindPath(Vector2 startPosition, Vector2 goalPosition)
    {
        Vector2Int startIndex = SafetyMap.Instance.ConvertWorldPositionToCellIndex(startPosition);
        Vector2Int goalIndex = SafetyMap.Instance.ConvertWorldPositionToCellIndex(goalPosition);

        Cell start = SafetyMap.Instance.GetCell(startIndex.x, startIndex.y);
        Cell goal = SafetyMap.Instance.GetCell(goalIndex.x, goalIndex.y);

        for (int y = 0; y < SafetyMap.Instance.gridSizeZ; y++)
        {
            for (int x = 0; x < SafetyMap.Instance.gridSizeX; x++)
            {
                Cell cell = SafetyMap.Instance.grid[y, x];
                cell.g = float.MaxValue;
                cell.h = 0.0f;
                cell.f = 0.0f;
                cell.parent = null;
            }
        }
        List<Cell> openList = new List<Cell>(); // Should be changed to a priority queue
        openList.Add(start);

        List<Cell> closedList = new List<Cell>();

        start.g = 0.0f;
        start.h = Heuristic(start, goal);
        start.f = start.g + start.h;

        while(openList.Count != 0)
        {
            Cell current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f < current.f)
                {
                    current = openList[i];
                }
            }

            if(current == goal)
            {
                List<Cell> path = ReconstructPath(goal);
                path.Reverse();
                return path;
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach(Vector2Int neighbourPosition in GetValidNeighbours(current))
            {
                Cell neighbor = SafetyMap.Instance.grid[neighbourPosition.y, neighbourPosition.x];

                if (closedList.Contains(neighbor)) continue;

                float tentativeG = current.g + Heuristic(current, neighbor);

                if (!openList.Contains(neighbor) || tentativeG < neighbor.g)
                {
                    neighbor.g = tentativeG;
                    neighbor.h = Heuristic(neighbor, goal);
                    neighbor.f = neighbor.g + neighbor.h;
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }
        return new List<Cell>();

    }
}
