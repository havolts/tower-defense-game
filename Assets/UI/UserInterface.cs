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
        meteorUpgrade.onClick.AddListener(AddMeteorSpellToHotbar);
        disperseUpgrade.onClick.AddListener(AddDisperseSpellToHotbar);
        suspendUpgrade.onClick.AddListener(AddSuspendSpellToHotbar);
        stoneUpgrade.onClick.AddListener(AddStoneSpellToHotbar);
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
            meteorUpgrade.onClick.RemoveListener(AddMeteorSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.meteorSpell.cost);
        }
    }
    void AddDisperseSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.disperseSpell.cost)
        {
            disperseSpellButton.SetActive(true);
            disperseUpgrade.onClick.RemoveListener(AddDisperseSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.disperseSpell.cost);
        }
    }
    void AddSuspendSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.suspendSpell.cost)
        {
            suspendSpellButton.SetActive(true);
            suspendUpgrade.onClick.RemoveListener(AddSuspendSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.suspendSpell.cost);
        }
    }
    void AddStoneSpellToHotbar()
    {
        if (SkillPoints.Instance.points >= SpellDatabase.Instance.stoneSpell.cost)
        {
            stoneSpellButton.SetActive(true);
            stoneUpgrade.onClick.RemoveListener(AddStoneSpellToHotbar);
            SkillPoints.Instance.Subtract(SpellDatabase.Instance.stoneSpell.cost);
        }
    }
}
