using UnityEngine;

public class Attacker : MonoBehaviour
{
    private int damage;
    private float attackRange;
    private float attackRate;

    public void Initialize(int dmg, float range, float rate)
    {
        damage = dmg;
        attackRange = range;
        attackRate = rate;
    }

    // Logic for finding and attacking targets will go here
    public void Attack(Health target)
    {
        if (target != null)
        {
            Debug.Log(gameObject.name + " is attacking " + target.name + " for " + damage + " damage.");
            target.TakeDamage(damage);
        }
    }
}
