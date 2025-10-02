using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public UnitData unitData;
    [HideInInspector] public UnitSpawner unitSpawner;
    [HideInInspector] public RectTransform panelRectTransform; 

    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector3 originalPosition;

    private bool isPlacingInWorld = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store original state to return to if canceled
        originalParent = transform.parent;
        originalPosition = transform.position;

        transform.SetParent(transform.root);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;

        // Check if the cursor is outside the original panel
        bool isOverPanel = RectTransformUtility.RectangleContainsScreenPoint(panelRectTransform, eventData.position);

        if (!isOverPanel && !isPlacingInWorld)
        {
            // We've just moved out of the panel, start world placement
            isPlacingInWorld = true;
            canvasGroup.alpha = 0f; 
            unitSpawner.BeginUnitPlacement(unitData);
        }
        else if (isOverPanel && isPlacingInWorld)
        {
            // We've moved back into the panel, cancel world placement
            isPlacingInWorld = false;
            canvasGroup.alpha = 1f;
            unitSpawner.CancelPlacement(); 
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlacingInWorld)
        {
            // If we release outside the panel, try to deploy
            unitSpawner.AttemptDeployment();
        }
        else
        {
            // If we release inside the panel, cancel everything
            unitSpawner.CancelPlacement();
        }

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        isPlacingInWorld = false;
    }
}