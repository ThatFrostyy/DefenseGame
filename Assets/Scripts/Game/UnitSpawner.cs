using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("Unit Prefab")]
    public GameObject unitPrefab;

    [Header("Animation Clips")]
    public string deployAnimationName = "";
    public string idleAnimationName = "";

    [Header("Effects & Sound")]
    public ParticleSystem deployEffect;
    public AudioClip[] deploySounds;

    [Header("Deployment Settings")]
    [Tooltip("Alpha value to use for preview (0 = invisible, 1 = solid).")]
    [Range(0f, 1f)] public float previewAlpha = 0.5f;
    public LayerMask deploymentLayerMask;
    public LayerMask obstacleMask;

    private bool isPreviewing = false;
    private bool canPlaceHere = false;
    private GameObject previewInstance;

    private Camera mainCamera;

    // Storage for cloned preview materials
    private readonly List<Material> clonedPreviewMats = new();

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            isPreviewing = !isPreviewing;

            if (isPreviewing) StartPreview();
            else EndPreview();
        }

        if (isPreviewing)
        {
            UpdatePreviewPosition();

            if (Input.GetMouseButtonDown(0))
            {
                DeployUnit();
            }
        }
    }

    private void StartPreview()
    {
        if (unitPrefab == null)
        {
            isPreviewing = false;
            return;
        }

        previewInstance = Instantiate(unitPrefab);

        // Disable legacy animation so it doesn’t start playing
        if (previewInstance.GetComponent<Animation>())
        {
            previewInstance.GetComponent<Animation>().enabled = false;
        }

        // Clone each material, set it to transparent, and reduce alpha
        Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            Material[] mats = rend.materials; // automatically instantiates
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];

                // Switch to transparent rendering
                mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent in URP Lit
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                // Enable blending
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);

                // Adjust alpha
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

        // Disable colliders on preview
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

        // Cleanup any cloned mats to avoid leaks
        foreach (var mat in clonedPreviewMats)
        {
            if (mat != null) Destroy(mat);
        }
        clonedPreviewMats.Clear();

        isPreviewing = false;
    }

    private void UpdatePreviewPosition()
    {
        if (previewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, deploymentLayerMask))
        {
            previewInstance.SetActive(true);
            previewInstance.transform.position = hit.point;

            Collider[] overlaps = Physics.OverlapBox(
                hit.point,
                new Vector3(1, 1, 1) * 0.5f,
                Quaternion.identity,
                obstacleMask
            );

            if (overlaps.Length > 0)
            {
                canPlaceHere = false;
                previewInstance.SetActive(false);
            }
            else
            {
                canPlaceHere = true;

            }
        }
        else
        {
            previewInstance.SetActive(false);
            canPlaceHere = false;
        }
    }

    private void DeployUnit()
    {
        if (!canPlaceHere)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, deploymentLayerMask))
        {
            Vector3 deployPosition = hit.point;

            GameObject newUnit = Instantiate(unitPrefab, deployPosition, Quaternion.identity);

            if (newUnit.TryGetComponent<Animation>(out var unitAnimation))
            {
                if (!string.IsNullOrEmpty(deployAnimationName))
                {
                    unitAnimation.Play(deployAnimationName);
                }
                if (!string.IsNullOrEmpty(idleAnimationName))
                {
                    unitAnimation.PlayQueued(idleAnimationName, QueueMode.CompleteOthers);
                }
            }

            if (deployEffect != null)
            {
                Instantiate(deployEffect, deployPosition, Quaternion.identity);
            }

            if (deploySounds != null && deploySounds.Length > 0)
            {
                foreach (AudioClip clip in deploySounds)
                {
                    if (clip != null)
                        AudioSource.PlayClipAtPoint(clip, deployPosition);
                }
            }

            EndPreview();
        }
    }
}