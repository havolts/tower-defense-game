using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class FortressMenu : MonoBehaviour
{
    public GameObject panel;
    public GameObject buttonPrefab;
    public GameObject wizardPrefab;
    public GameObject spawnPoint;
    public GameObject Fortress;
    public Camera camera;

    private bool buttonsVisible = false;
    public GameObject stonePrefab;
    private bool placingStone = false;
    public LayerMask groundLayer;


    void Update()
    {
            if (Input.GetMouseButtonDown(0))
        {
            // Skip if clicking on UI
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (placingStone)
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Only hit the ground
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
                {
                    Instantiate(stonePrefab, hit.point, Quaternion.identity);
                    Debug.Log("Stone placed at " + hit.point);
                }
                else
                {
                    Debug.Log("Must click on the ground.");
                }

                placingStone = false;
                return;
            }

            Ray rayClick = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitClick;

            if (Physics.Raycast(rayClick, out hitClick))
            {
                if (hitClick.collider.gameObject == Fortress)
                {
                    if (!buttonsVisible)
                        ShowButtons();
                }
                else
                {
                    if (buttonsVisible)
                        HideButtons();
                }
            }
            else
            {
                if (buttonsVisible)
                    HideButtons();
            }
        }

    }

    void ShowButtons()
    {
        ClearPanel();

        CreateButton("Summon Wizard", SpawnWizard);
        CreateButton("Place Stone Barrier", PlaceStone);

        buttonsVisible = true;
    }

    void HideButtons()
    {
        ClearPanel();
        buttonsVisible = false;
    }

    void ClearPanel()
    {
        foreach (Transform child in panel.transform)
            Destroy(child.gameObject);
    }

    void CreateButton(string label, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = Instantiate(buttonPrefab, panel.transform);
        obj.transform.SetParent(panel.transform, false);

        Button button = obj.GetComponent<Button>();
        Text text = obj.GetComponentInChildren<Text>();
        text.text = label;
        button.onClick.AddListener(action);
    }

    void SpawnWizard()
    {
        Instantiate(wizardPrefab, spawnPoint.transform.position, Quaternion.identity);
        Debug.Log("Wizard spawned.");
    }

    void PlaceStone()
    {
        Debug.Log("Click on the battlefield to place stone.");
        placingStone = true;
    }

}
