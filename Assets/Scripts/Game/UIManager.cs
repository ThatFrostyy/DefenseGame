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

        CreateUnitCards();
    }

    void CreateUnitCards()
    {
        foreach (var unitData in availableUnits)
        {
            GameObject buttonGO = Instantiate(unitButtonPrefab, unitSelectionPanel);

            if (buttonGO.TryGetComponent<UnitCardUI>(out var card))
            {
                card.Setup(unitData, rarityFrameData);
            }

            if (buttonGO.TryGetComponent<DragHandler>(out var handler))
            {
                handler.unitData = unitData;
                handler.unitSpawner = unitSpawner;
                handler.panelRectTransform = unitSelectionPanel as RectTransform; 
            }
        }
    }
}