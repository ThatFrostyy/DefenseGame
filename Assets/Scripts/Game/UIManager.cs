using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform unitSelectionPanel;
    public GameObject unitButtonPrefab;

    [Header("System References")]
    public RarityFrameData rarityFrameData; 

    [Header("Units")]
    public List<UnitData> availableUnits;

    private UnitSpawner unitSpawner;

    void Start()
    {
        unitSpawner = GetComponent<UnitSpawner>();

        if (unitSpawner == null || rarityFrameData == null) 
        {
            Debug.LogError("System references are not set in the UIManager!");
            return;
        }

        CreateUnitButtons();
    }

    void CreateUnitButtons()
    {
        foreach (var unitData in availableUnits)
        {
            GameObject buttonGO = Instantiate(unitButtonPrefab, unitSelectionPanel);

            // Get the UnitCardUI component and set it up
            if (buttonGO.TryGetComponent<UnitCardUI>(out var card))
            {
                card.Setup(unitData, rarityFrameData);
            }

            if (buttonGO.TryGetComponent<Button>(out var button))
            {
                button.onClick.AddListener(() => {
                    unitSpawner.BeginUnitPlacement(unitData);
                });
            }
        }
    }
}