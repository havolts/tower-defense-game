public class Spell
{
    public SpellType type;
    public int cost;
    public int level;
    public float damage;
    public float radius;
    public float cooldown;

    public Spell(SpellType _type, int _cost, int _level, float _damage, float _radius, float _cooldown)
    {
        type = _type;
        cost = _cost;
        level = _level;
        damage = _damage;
        radius = _radius;
        cooldown = _cooldown;
    }
}

public enum SpellType {Meteor, Disperse, Suspend, Stone}
