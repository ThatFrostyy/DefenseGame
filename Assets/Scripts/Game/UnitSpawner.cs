using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("Deployment Settings")]
    [Tooltip("Alpha value to use for preview (0 = invisible, 1 = solid).")]
    [Range(0f, 1f)] public float previewAlpha = 0.5f;
    public LayerMask deploymentLayerMask;
    public LayerMask obstacleMask;

    private UnitData currentUnitToPlace;

    private bool isPreviewing = false;
    private bool canPlaceHere = false;
    private GameObject previewInstance;

    private Camera mainCamera;

    private readonly List<Material> clonedPreviewMats = new();

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (isPreviewing)
        {
            UpdatePreviewPosition();

            if (Input.GetMouseButtonDown(0))
            {
                DeployUnit();
            }

            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    /// <summary>
    /// Called by UI Manager when player selects a unit to deploy.
    /// </summary>
    public void BeginUnitPlacement(UnitData unitData)
    {
        if (isPreviewing)
        {
            CancelPlacement();
        }

        if (unitData == null || unitData.unitPrefab == null) return;

        currentUnitToPlace = unitData;
        isPreviewing = true;
        StartPreview();
    }

    public void CancelPlacement()
    {
        EndPreview();
    }

    private void StartPreview()
    {
        previewInstance = Instantiate(currentUnitToPlace.unitPrefab);

        if (previewInstance.GetComponent<Animation>())
        {
            previewInstance.GetComponent<Animation>().enabled = false;
        }

        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                mat.SetFloat("_Surface", 1);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);

                if (mat.HasProperty("_BaseColor"))
                {
                    Color col = mat.GetColor("_BaseColor");
                    col.a = previewAlpha;
                    mat.SetColor("_BaseColor", col);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color col = mat.GetColor("_Color");
                    col.a = previewAlpha;
                    mat.SetColor("_Color", col);
                }

                mats[i] = mat;
                clonedPreviewMats.Add(mat);
            }
            rend.materials = mats;
        }

        foreach (Collider col in previewInstance.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
    }

    private void EndPreview()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }
        previewInstance = null;

        foreach (var mat in clonedPreviewMats)
        {
            if (mat != null) Destroy(mat);
        }
        clonedPreviewMats.Clear();

        isPreviewing = false;
        currentUnitToPlace = null;
    }

    private void UpdatePreviewPosition()
    {
        if (previewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, deploymentLayerMask))
        {
            previewInstance.transform.position = hit.point;

            Collider[] overlaps = Physics.OverlapBox(hit.point, new Vector3(1, 1, 1) * 0.5f, Quaternion.identity, obstacleMask);
            canPlaceHere = overlaps.Length == 0;

            previewInstance.SetActive(canPlaceHere);
        }
        else
        {
            previewInstance.SetActive(false);
            canPlaceHere = false;
        }
    }

    private void DeployUnit()
    {
        if (!canPlaceHere) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, deploymentLayerMask))
        {
            Vector3 deployPosition = hit.point;
            GameObject newUnit = Instantiate(currentUnitToPlace.unitPrefab, deployPosition, Quaternion.identity);

            if (newUnit.TryGetComponent<Animation>(out var unitAnimation))
            {
                if (currentUnitToPlace.animationData.appear != null)
                {
                    unitAnimation.Play(currentUnitToPlace.animationData.appear.name); 
                }

                if (currentUnitToPlace.animationData.idle != null)
                {
                    unitAnimation.PlayQueued(currentUnitToPlace.animationData.idle.name, QueueMode.CompleteOthers);
                }
            }        

            if (currentUnitToPlace.deployEffect != null)
            {
                Instantiate(currentUnitToPlace.deployEffect, deployPosition, Quaternion.identity);
            }

            if (currentUnitToPlace.deploySounds != null && currentUnitToPlace.deploySounds.Length > 0)
            {
                foreach (AudioClip clip in currentUnitToPlace.deploySounds)
                {
                    if (clip != null) AudioSource.PlayClipAtPoint(clip, deployPosition);
                }
            }

            EndPreview();
        }
    }
}