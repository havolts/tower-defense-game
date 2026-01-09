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
    public Camera cam;
    public SkillPoints playerSkillPoints;

    private bool buttonVisible = false;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == Fortress)
            {
                if (!buttonVisible) ShowButton();
            }
            else if (buttonVisible)
            {
                HideButton();
            }
        }
        else if (buttonVisible)
        {
            HideButton();
        }
    }

    void ShowButton()
    {
        ClearPanel();

        GameObject btnObj = Instantiate(buttonPrefab, panel.transform);
        btnObj.transform.SetParent(panel.transform, false);

        // Ensure correct scale
        btnObj.transform.localScale = Vector3.one;

        Button button = btnObj.GetComponent<Button>();
        button.onClick.AddListener(TrySpawnWizard);

        buttonVisible = true;
    }

    void HideButton()
    {
        ClearPanel();
        buttonVisible = false;
    }

    void ClearPanel()
    {
        foreach (Transform child in panel.transform)
            Destroy(child.gameObject);
    }

    void TrySpawnWizard()
    {
        if (playerSkillPoints.points >= 2)
        {
            playerSkillPoints.Add(-2);
            Instantiate(wizardPrefab, spawnPoint.transform.position, Quaternion.identity);
        }
        else
        {
            StartCoroutine(FlashButtonRed(panel.GetComponentInChildren<Button>()));
        }
    }

    IEnumerator FlashButtonRed(Button button)
    {
        Color original = button.image.color;
        button.image.color = Color.red;
        yield return new WaitForSeconds(1f);
        button.image.color = original;
    }
}
