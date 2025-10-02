using UnityEngine;
using UnityEngine.Events; 

public class Health : MonoBehaviour
{
    public int CurrentHealth { get; private set; }

    // An event that other scripts can listen to, for when the unit dies.
    [HideInInspector] public UnityEvent OnDeath;

    private int maxHealth;

    private Unit unit;
    private Animation anim;

    private void Start()
    {
        unit = GetComponent<Unit>();
        anim = GetComponent<Animation>();
    }

    public void Initialize(int startingHealth)
    {
        maxHealth = startingHealth;
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= amount;

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        OnDeath.Invoke(); // Fire the death event

        anim.Play(unit.UnitData.animationData.GetRandomDeathClip().name);

        Destroy(gameObject, 2f);
    }
}
