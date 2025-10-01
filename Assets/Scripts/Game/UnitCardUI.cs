using UnityEngine;
using UnityEngine.UI;

public class UnitCardUI : MonoBehaviour
{
    public Image iconImage;
    public Image frameImage;

    public void Setup(UnitData unitData, RarityFrameData frameData)
    {
        if (unitData == null || frameData == null) return;

        if (iconImage != null)
        {
            iconImage.sprite = unitData.uiIcon;
        }

        if (frameImage != null)
        {
            frameImage.sprite = frameData.GetFrameForRarity(unitData.rarity);
        }
    }
}