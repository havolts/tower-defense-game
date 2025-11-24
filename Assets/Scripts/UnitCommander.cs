using System.Collections.Generic;
using UnityEngine;

public class UnitCommander : MonoBehaviour
{
    public Camera cam;
    public LayerMask obstacleMask;
    public LayerMask groundMask;
    public LayerMask FriendlyUnitMask;
    public LayerMask EnemyUnitMask;
    public GameObject selectionIndicatorPrefab;

    private List<FriendlyUnit> selected = new List<FriendlyUnit>();
    private Dictionary<FriendlyUnit, GameObject> indicators = new Dictionary<FriendlyUnit, GameObject>();

    // Box selection
    private bool isDragging = false;
    private Vector2 dragStartPos;
    private Rect selectionRect;

    void Update()
    {
        HandleSelection();
        HandleOrders();
        UpdateIndicators();
    }

    void HandleSelection()
    {
        // Begin potential drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        // End drag or click
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                float dragDistance = (Input.mousePosition - (Vector3)dragStartPos).magnitude;

                // If mouse barely moved → treat as click
                if (dragDistance < 10f)
                    HandleClickSelection();
                else
                    SelectUnitsInBox();

                isDragging = false;
            }
        }
    }

    void HandleClickSelection()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, obstacleMask | FriendlyUnitMask | groundMask))
            return;

        int hitLayer = hit.collider.gameObject.layer;

        if (((1 << hitLayer) & FriendlyUnitMask) != 0)
        {
            FriendlyUnit unit = hit.collider.GetComponent<FriendlyUnit>();
            if (unit == null) return;

            // Clear selection unless Shift held
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                foreach (var u in selected) RemoveIndicator(u);
                selected.Clear();
            }

            if (!selected.Contains(unit))
                selected.Add(unit);
        }
        else if (((1 << hitLayer) & groundMask) != 0)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                foreach (var u in selected) RemoveIndicator(u);
                selected.Clear();
            }
        }
    }


    void SelectUnitsInBox()
    {
        Vector2 endPos = Input.mousePosition;

        selectionRect = new Rect(
            Mathf.Min(dragStartPos.x, endPos.x),
            Mathf.Min(dragStartPos.y, endPos.y),
            Mathf.Abs(dragStartPos.x - endPos.x),
            Mathf.Abs(dragStartPos.y - endPos.y)
        );

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            foreach (var u in selected)
                RemoveIndicator(u);
            selected.Clear();
        }

        foreach (var unit in FindObjectsByType<FriendlyUnit>(FindObjectsSortMode.None))
        {
            Vector3 screenPos = cam.WorldToScreenPoint(unit.transform.position);
            if (selectionRect.Contains(screenPos, true))
            {
                if (!selected.Contains(unit))
                    selected.Add(unit);
            }
        }
    }


    void OnGUI()
    {
        if (isDragging)
        {
            var rect = GetScreenRect(dragStartPos, Input.mousePosition);
            DrawScreenRectBorder(rect, 2, Color.green);
        }
    }

    static Rect GetScreenRect(Vector2 start, Vector2 end)
    {
        start.y = Screen.height - start.y;
        end.y = Screen.height - end.y;
        Vector2 topLeft = Vector2.Min(start, end);
        Vector2 bottomRight = Vector2.Max(start, end);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        Color prev = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = prev;
    }

    void HandleOrders()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, obstacleMask | EnemyUnitMask | groundMask))
            return;

        int hitLayer = hit.collider.gameObject.layer;

        if (((1 << hitLayer) & EnemyUnitMask) != 0)
        {
            EnemyUnit enemy = hit.collider.GetComponent<EnemyUnit>();
            if (enemy == null) return;

            foreach (var selectedUnit in selected)
                selectedUnit.AddOrder(new Order(OrderType.Attack, enemy.transform, true), append: Input.GetKey(KeyCode.LeftShift));
        }
        else if (((1 << hitLayer) & groundMask) != 0)
        {
            Vector3 targetPosition = hit.point;
            foreach (var selectedUnit in selected)
                selectedUnit.AddOrder(new Order(OrderType.Move, targetPosition, true), append: Input.GetKey(KeyCode.LeftShift));
        }
    }

    void UpdateIndicators()
    {
        foreach (var unit in selected)
        {
            if (!indicators.ContainsKey(unit))
            {
                GameObject indicator = Instantiate(selectionIndicatorPrefab);
                indicator.transform.SetParent(unit.transform);
                indicator.transform.localPosition = Vector3.up * 2f;
                indicators[unit] = indicator;
            }
        }
    }

    void RemoveIndicator(FriendlyUnit unit)
    {
        if (indicators.ContainsKey(unit))
        {
            Destroy(indicators[unit]);
            indicators.Remove(unit);
        }
    }
}
