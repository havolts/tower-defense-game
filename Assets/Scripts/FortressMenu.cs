using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class FortressMenu : MonoBehaviour
{
    public GameObject panel;
    public GameObject buttonPrefab;
    public GameObject wizardPrefab;
    public GameObject spawnPoint;
    public GameObject Fortress;
    public Camera camera;
    public GameObject stonePrefab;
    public LayerMask groundLayer;

    public SkillPoints playerSkillPoints;

    private bool buttonsVisible = false;
    private bool placingStone = false;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (placingStone)
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Instantiate(stonePrefab, hit.point, Quaternion.identity);
                //Debug.Log("Stone placed at " + hit.point);
            }
            else
            {
                Debug.Log("Must click on the ground.");
            }
            placingStone = false;
            return;
        }

        Ray rayClick = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayClick, out RaycastHit hitClick))
        {
            if (hitClick.collider.gameObject == Fortress)
            {
                if (!buttonsVisible) ShowButtons();
            }
            else
            {
                if (buttonsVisible) HideButtons();
            }
        }
        else
        {
            if (buttonsVisible) HideButtons();
        }
    }

    void ShowButtons()
    {
        ClearPanel();
        CreateButton("Summon Wizard (2 SP)", () => AttemptAction(2, SpawnWizard));
        CreateButton("Place Stone Barrier (4 SP)", () => AttemptAction(4, PlaceStone));
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

    void AttemptAction(int cost, UnityEngine.Events.UnityAction action)
    {
        Debug.Log(playerSkillPoints.Points);
        Button clickedButton = null;

        foreach (Transform child in panel.transform)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null && btn.onClick.GetPersistentEventCount() > 0)
            {
                for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
                {
                    var targetMethod = btn.onClick.GetPersistentMethodName(i);
                    if (targetMethod == action.Method.Name)
                    {
                        clickedButton = btn;
                        break;
                    }
                }
            }
            if (clickedButton != null) break;
        }

        if (playerSkillPoints.Points >= cost)
        {
            playerSkillPoints.Add(-cost);
            action.Invoke();
        }
        else
        {
            if (clickedButton != null) StartCoroutine(FlashButtonRed(clickedButton));
        }
    }

    IEnumerator FlashButtonRed(Button button)
    {
        Color original = button.image.color;
        button.image.color = Color.red;
        yield return new WaitForSeconds(1f);
        button.image.color = original;
    }

    void SpawnWizard()
    {
        Instantiate(wizardPrefab, spawnPoint.transform.position, Quaternion.identity);
        //Debug.Log("Wizard spawned.");
    }

    void PlaceStone()
    {
        //Debug.Log("Click on the battlefield to place stone.");
        placingStone = true;
    }
}
