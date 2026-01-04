using UnityEngine;

public class Cell
{
    public Vector2Int index;
    public Vector2 position;
    public float friendlyInfluence = 0.0f;
    public float friendlyProximity = 0.0f;
    public float enemyInfluence = 0.0f;
    public float enemyProximity = 0.0f;

    public float g = float.MaxValue;
    public float h  = 0.0f;
    public float f;
    public Cell parent;

    public Cell(int x, int y)
    {
        index = new Vector2Int(x, y);
        position = SafetyMap.Instance.ConvertCellIndexToWorldPosition(new Vector2Int(x,y));
    }
}

public enum CellData { friendlyInfluence, friendlyProximity, enemyInfluence, enemyProximity }
