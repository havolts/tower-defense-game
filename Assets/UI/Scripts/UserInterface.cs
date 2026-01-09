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

    public Button endGameSpell;
    public GameObject endGameSpellPrefab;
    public int endgameCost = 50;

    public CameraController cam;

    bool panelOpen = false;

    public GameObject fortress;
    Health fortressHealth;
    float fortressCurrentHealth;

    void Start()
    {
        fortressHealth = fortress.GetComponent<Health>();
        fortressCurrentHealth = fortressHealth.currentHealth;
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
        endGameSpell.onClick.AddListener(EndGame);

        UpdateSpellCostText(meteorUpgrade.gameObject, SpellDatabase.Instance.meteorSpell.cost);
        UpdateSpellCostText(disperseUpgrade.gameObject, SpellDatabase.Instance.disperseSpell.cost);
        UpdateSpellCostText(suspendUpgrade.gameObject, SpellDatabase.Instance.suspendSpell.cost);
        UpdateSpellCostText(stoneUpgrade.gameObject, SpellDatabase.Instance.stoneSpell.cost);
        UpdateSpellCostText(endGameSpell.gameObject, endgameCost);
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
        fortressCurrentHealth = fortressHealth.currentHealth;
        DisableSpells();
        points.text = SkillPoints.Instance.points.ToString();
        UpdateCooldownVisuals();
    }

    void DisableSpells()
    {
        if(fortressCurrentHealth < ((fortressHealth.stats.maxHealth/4)*3))
        {
            meteorSpellButton.SetActive(false);
        }
        if(fortressCurrentHealth < ((fortressHealth.stats.maxHealth/4)*2))
        {
            suspendSpellButton.SetActive(false);
        }
        if(fortressCurrentHealth < ((fortressHealth.stats.maxHealth/4)))
        {
            disperseSpellButton.SetActive(false);
        }
        if(fortressCurrentHealth < ((fortressHealth.stats.maxHealth/5)))
        {
            stoneSpellButton.SetActive(false);
        }
    }

    void UpdateCooldownVisuals()
    {
        UpdateSpellButtonColor(meteorSpellButton, SpellType.Meteor);
        UpdateSpellButtonColor(disperseSpellButton, SpellType.Disperse);
        UpdateSpellButtonColor(suspendSpellButton, SpellType.Suspend);
        UpdateSpellButtonColor(stoneSpellButton, SpellType.Stone);
    }

    void UpdateSpellButtonColor(GameObject spellButton, SpellType type)
    {
        CastSpell cast = spellButton.GetComponent<CastSpell>();
        if (cast == null) return;

        bool coolingDown = !cast.IsCooldownOver(type);
        Image img = spellButton.GetComponent<Image>();
        if (img != null)
            img.color = coolingDown ? Color.grey : Color.white;
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

    void EndGame()
    {
        if (SkillPoints.Instance.points >= endgameCost)
        {
            SkillPoints.Instance.Subtract(endgameCost);
            panel.SetActive(false);
            GameController.Instance.StopAllUnits();
            var spell = Instantiate(endGameSpellPrefab);
            cam.frozen = true;
            cam.followTarget = spell.transform;
        }
    }
}
