using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Unit Stats")]
public class UnitStats : ScriptableObject
{
    public float attackDamage;
    public float attackCooldown;
    public float attackRange;

    public float movementSpeed;
}
