using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour
{
    int unitUpgradeIndex;
    public TMP_Text points;
    public GameObject panel;
    public Button upgradePanelToggle;

    public GameObject meteorSpellButton;
    public Button meteorUpgrade;

    public GameObject disperseSpellButton;
    public Button disperseUpgrade;

    public GameObject suspendSpellButton;
    public Button suspendUpgrade;

    public GameObject stoneSpellButton;
    public Button stoneUpgrade;


    bool panelOpen = false;

    void Start()
    {
        panel.SetActive(false);
        upgradePanelToggle.onClick.AddListener(TogglePanel);

        meteorUpgrade.image.color = Color.grey;
        disperseUpgrade.image.color = Color.grey;
        suspendUpgrade.image.color = Color.grey;
        stoneUpgrade.image.color = Color.grey;

        meteorUpgrade.onClick.AddListener(AddMeteorSpellToHotbar);
        disperseUpgrade.onClick.AddListener(AddDisperseSpellToHotbar);
        suspendUpgrade.onClick.AddListener(AddSuspendSpellToHotbar);
        stoneUpgrade.onClick.AddListener(AddStoneSpellToHotbar);

        UpdateSpellCostText(meteorUpgrade.gameObject, SpellDatabase.Instance.meteorSpell.cost);
        UpdateSpellCostText(disperseUpgrade.gameObject, SpellDatabase.Instance.disperseSpell.cost);
        UpdateSpellCostText(suspendUpgrade.gameObject, SpellDatabase.Instance.suspendSpell.cost);
        UpdateSpellCostText(stoneUpgrade.gameObject, SpellDatabase.Instance.stoneSpell.cost);
    }

    void UpdateSpellCostText(GameObject button, int cost)
    {
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text += " (" + cost + ") ";
        }
    }



    void Update()
    {
        points.text = SkillPoints.Instance.points.ToString();
    }

    void TogglePanel()
    {
        if(panelOpen)
        {
            panel.SetActive(false);
            panelOpen = false;
        }
        else
        {
            panel.SetActive(true);
            panelOpen = true;
        }
    }

    void AddMeteorSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.meteorSpell.cost)
        {
            meteorSpellButton.SetActive(true);
            meteorUpgrade.image.color = Color.white; // restore normal color
            meteorUpgrade.onClick.RemoveListener(AddMeteorSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.meteorSpell.cost);
        }
    }

    void AddDisperseSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.disperseSpell.cost)
        {
            disperseSpellButton.SetActive(true);
            disperseUpgrade.image.color = Color.white;
            disperseUpgrade.onClick.RemoveListener(AddDisperseSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.disperseSpell.cost);
        }
    }
    void AddSuspendSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.suspendSpell.cost)
        {
            suspendSpellButton.SetActive(true);
            suspendUpgrade.image.color = Color.white;
            suspendUpgrade.onClick.RemoveListener(AddSuspendSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.suspendSpell.cost);
        }
    }
    void AddStoneSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.stoneSpell.cost)
        {
            stoneSpellButton.SetActive(true);
            stoneUpgrade.image.color = Color.white;
            stoneUpgrade.onClick.RemoveListener(AddStoneSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.stoneSpell.cost);
        }
    }
}
