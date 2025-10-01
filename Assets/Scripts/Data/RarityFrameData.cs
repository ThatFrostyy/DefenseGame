using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "RarityFrameData", menuName = "RtV/Rarity Frame Data", order = 2)]
public class RarityFrameData : ScriptableObject
{
    // This is a helper class to make the list look nice in the Inspector
    [System.Serializable]
    public class RarityFrameMapping
    {
        public UnitRarity rarity;
        public Sprite frameSprite;
    }

    // A list of all our rarity-to-frame mappings
    public List<RarityFrameMapping> rarityFrames;

    public Sprite GetFrameForRarity(UnitRarity rarity)
    {
        // Find the mapping in our list that matches the requested rarity
        // and return its sprite. If not found, return null.
        return rarityFrames.FirstOrDefault(mapping => mapping.rarity == rarity)?.frameSprite;
    }
}