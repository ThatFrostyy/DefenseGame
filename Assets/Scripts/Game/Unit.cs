using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData UnitData { get; private set; }

    public Health Health { get; private set; }
    public Mover Mover { get; private set; }
    public Attacker Attacker { get; private set; }

    void Awake()
    {
        Health = GetComponent<Health>();
        Mover = GetComponent<Mover>();
        Attacker = GetComponent<Attacker>();
    }

    // Called by spawner right after instantiation
    public void Setup(UnitData data)
    {
        UnitData = data;

        if (Health != null)
        {
            Health.Initialize(UnitData.maxHealth);
        }

        if (Mover != null)
        {
            Mover.Initialize(UnitData.moveSpeed);
        }

        if (Attacker != null)
        {
            Attacker.Initialize(UnitData.damage, UnitData.attackRange, UnitData.attackRate);
        }

        gameObject.name = UnitData.unitName;
    }
}
