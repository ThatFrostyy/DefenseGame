using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSpawner : MonoBehaviour
{
    [Header("Deployment Settings")]
    [Tooltip("Alpha value to use for preview (0 = invisible, 1 = solid).")]
    [Range(0f, 1f)] public float previewAlpha = 0.5f;
    public LayerMask deploymentLayerMask;
    public LayerMask obstacleMask;
    public GameObject squadPrefab; // Assign a prefab with the Squad script here

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

            if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    // Called by UI Manager when player selects a unit to deploy
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

    public void AttemptDeployment()
    {
        if (canPlaceHere)
        {
            Vector3 deployPosition = previewInstance.transform.position;
            UnitData unitToDeploy = currentUnitToPlace;

            StartCoroutine(DeployWithDelay(unitToDeploy, deployPosition));

            EndPreview();
        }
        else
        {
            CancelPlacement();
        }
    }

    private IEnumerator DeployWithDelay(UnitData unitData, Vector3 deployPosition)
    {
        // Can instantiate a visual effect here (like a parachute shadow or a construction site)
        // to show that something is happening at `deployPosition`.

        yield return new WaitForSeconds(unitData.deployTime);

        GameObject newSquadGO = Instantiate(squadPrefab, deployPosition, Quaternion.identity);
        Squad newSquad = newSquadGO.GetComponent<Squad>();

        newSquadGO.name = $"{unitData.cardName}";

        for (int i = 0; i < newSquad.formationPoints.Count; i++)
        {
            if (i >= unitData.squadSize) break;

            Vector3 spawnPos = newSquad.formationPoints[i].position;

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                SpawnSingleSoldier(hit.position, newSquad, unitData);
            }
            else
            {
                Debug.LogWarning($"Could not find valid NavMesh position for formation point {i}. Spawning at squad center.");
            }
        }

        if (unitData.deployEffect != null)
        {
            Instantiate(unitData.deployEffect, deployPosition, Quaternion.identity);
        }

        if (unitData.deploySounds != null && unitData.deploySounds.Length > 0)
        {
            foreach (AudioClip clip in unitData.deploySounds)
            {
                if (clip != null) AudioSource.PlayClipAtPoint(clip, deployPosition);
            }
        }
    }

    public void CancelPlacement()
    {
        EndPreview();
    }

    private void SpawnSingleSoldier(Vector3 position, Squad squad, UnitData unitData)
    {
        GameObject newUnitGO = Instantiate(unitData.unitPrefab, position, Quaternion.identity);
        newUnitGO.transform.SetParent(squad.transform);

        if (newUnitGO.TryGetComponent<Soldier>(out var soldierComponent))
        {
            soldierComponent.Setup(unitData);
            squad.AddSoldier(soldierComponent);
        }

        if (newUnitGO.TryGetComponent<UnitAnimation>(out var unitAnimation))
        {
            unitAnimation.Setup(unitData);
            unitAnimation.PlayAppear();
        }
    }

    #region Preview
    private void StartPreview()
    {
        previewInstance = Instantiate(currentUnitToPlace.unitPrefab);

        if (previewInstance.TryGetComponent<Soldier>(out var soldier))
        {
            soldier.enabled = false;
        }
        if (previewInstance.TryGetComponent<UnitAnimation>(out var anim))
        {
            anim.enabled = false;
        }
        if (previewInstance.TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = false;
        }

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
                Material mat = new(mats[i]); // Create a new material instance
                mat.SetFloat("_Surface", 1); // For URP Lit shader to enable transparency
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
    #endregion Preview
}