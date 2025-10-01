using UnityEngine;

public enum UnitType { Infantry, Vehicle, Tank, Building}
public enum UnitRarity { Common, Rare, Elite, Epic }

[CreateAssetMenu(fileName = "NewUnitData", menuName = "RtV/Unit Data", order = 1)]
public class UnitData : ScriptableObject
{
    [Header("Unit Information")]
    public string unitName;
    public UnitType unitType;
    public UnitRarity rarity;
    public Sprite uiIcon;

    [Header("Game Object & Animations")]
    public GameObject unitPrefab;
    public string deployAnimationName = "";
    public string idleAnimationName = "";

    [Header("Effects & Sound")]
    public ParticleSystem deployEffect;
    public AudioClip[] deploySounds;
}