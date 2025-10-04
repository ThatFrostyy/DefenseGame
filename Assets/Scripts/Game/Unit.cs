using UnityEngine;

public class Unit : MonoBehaviour
{
    public UnitData UnitData { get; private set; }

    public Health Health { get; private set; }

    void Awake()
    {
        Health = GetComponent<Health>();
    }

    // Called by spawner right after instantiation
    public void Setup(UnitData data)
    {
        UnitData = data;

        if (Health != null)
        {
            Health.Initialize(UnitData.maxHealth);
        }

        gameObject.name = UnitData.unitName;
    }
}
