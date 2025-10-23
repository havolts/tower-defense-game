using System.Collections.Generic;
using UnityEngine;

public class UnitCommander : MonoBehaviour
{
    public Camera cam;
    public LayerMask obstacleMask;
    public LayerMask groundMask;
    public LayerMask FriendlyUnitMask;
    public LayerMask EnemyUnitMask;
    public GameObject selectionIndicatorPrefab; // assign a small cube prefab in inspector

    private List<FriendlyUnit> selected = new List<FriendlyUnit>();
    private Dictionary<FriendlyUnit, GameObject> indicators = new Dictionary<FriendlyUnit, GameObject>();

    void Update()
    {
        HandleSelection();
        HandleOrders();
        HandleMoveToMouseShortcut();
        UpdateIndicators();
    }

    void HandleSelection()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, obstacleMask | FriendlyUnitMask | groundMask))
            return;

        int hitLayer = hit.collider.gameObject.layer;

        if (((1 << hitLayer) & FriendlyUnitMask) != 0)
        {
            FriendlyUnit unit = hit.collider.GetComponent<FriendlyUnit>();
            if (unit == null) return;

            if (!Input.GetKey(KeyCode.LeftShift))
            {
                foreach (var u in selected)
                    RemoveIndicator(u);

                selected.Clear();
            }

            if (!selected.Contains(unit))
                selected.Add(unit);
        }
        else if (((1 << hitLayer) & groundMask) != 0)
        {
            foreach (var u in selected)
                RemoveIndicator(u);

            selected.Clear();
        }
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
                selectedUnit.AddOrder(new Order(OrderType.Attack, enemy.transform), append: Input.GetKey(KeyCode.LeftShift));
        }
        else if (((1 << hitLayer) & groundMask) != 0)
        {
            Vector3 targetPosition = hit.point;
            foreach (var selectedUnit in selected)
                selectedUnit.AddOrder(new Order(OrderType.Move, targetPosition), append: Input.GetKey(KeyCode.LeftShift));
        }
    }

    void HandleMoveToMouseShortcut()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundMask))
        {
            Vector3 targetPosition = hit.point;
            foreach (var selectedUnit in selected)
            {
                bool append = Input.GetKey(KeyCode.LeftShift);
                selectedUnit.AddOrder(new Order(OrderType.Move, hit.point), append);

            }
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
                indicator.transform.localPosition = Vector3.up * 2f; // above head
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
