using UnityEngine;

public enum UnitType { Infantry, Vehicle, Tank, Building}
public enum UnitRarity { Common, Rare, Elite, Epic }

[CreateAssetMenu(fileName = "NewUnitData", menuName = "RtV/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Unit Information")]
    public string unitName;
    public GameObject unitPrefab;
    public UnitType unitType;
    public UnitRarity rarity;
    public Sprite uiIcon;

    [Header("Gameplay Stats")]
    public int cost;
    public float deployTime;
    public int maxHealth;
    public int damage;
    public float attackRange;
    public float attackRate; // Attacks per second
    public float moveSpeed; // Note: 0 for a building

    [Header("Animation Data")]
    public UnitAnimationData animationData;

    [Header("Effects & Sound")]
    public ParticleSystem deployEffect;
    public AudioClip[] deploySounds;
}