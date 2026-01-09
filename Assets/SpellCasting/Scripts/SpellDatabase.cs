using UnityEngine;

public class SpellDatabase : MonoBehaviour
{
    public static SpellDatabase Instance { get; private set; }

    public Spell meteorSpell = new Spell(SpellType.Meteor, 15, 30f, 3f, 20f);
    public Spell disperseSpell = new Spell(SpellType.Disperse, 5, 0f, 5f, 10f);
    public Spell suspendSpell = new Spell(SpellType.Suspend, 5, 0f, 10f, 10f);
    public Spell stoneSpell = new Spell(SpellType.Stone, 10, 0, 0, 10f);

    private void Awake()
    {
        if (Instance == null) Instance = this; else Destroy(gameObject);
    }
}
