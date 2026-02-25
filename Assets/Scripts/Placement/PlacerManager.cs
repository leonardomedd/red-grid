using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlacerManager : MonoBehaviour
{
    public static PlacerManager Instance;

    [Header("Resources")]
    public int currentRecruitment = 50; // Aumentado para permitir mais unidades no início

    [Header("Placement rules")]
    public LayerMask placementBlockMask; // Layers that block placement (Units+Structures)
    public LayerMask unitsLayerMask;     // Units layer for zone cap check
    public float placementCheckRadius = 0.6f;
    public float zoneRadius = 2.0f;
    public int maxPerZone = 6;

    [Header("Build")]
    public float defaultBuildTime = 2.0f;
    public Transform unitsContainer;
    public Transform structuresContainer;

    // UI refs (assign)
    public TMP_Text recruitmentText; // UI TextMeshPro to show recruitment
    public GameObject buildProgressPrefab; // world-space small canvas with Image fill

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        Instance = this;
        UpdateRecruitmentUI();
    }

    public bool CanAfford(int cost)
    {
        return currentRecruitment >= cost;
    }

    public bool ValidatePlacement(Vector2 worldPos, float footprintRadius)
    {
        // collision with blocking layers
        var hits = Physics2D.OverlapCircleAll(worldPos, footprintRadius, placementBlockMask);
        if (hits.Length > 0) return false;

        // zone cap: count nearby units (unitsLayerMask)
        var nearby = Physics2D.OverlapCircleAll(worldPos, zoneRadius, unitsLayerMask);
        if (nearby.Length >= maxPerZone) return false;

        return true;
    }

    public void RequestPlace(GameObject prefab, Vector2 worldPos, int cost, bool isStructure)
    {
        RequestPlaceMultiple(prefab, worldPos, cost, isStructure, 1);
    }

    public void RequestPlaceMultiple(GameObject prefab, Vector2 worldPos, int cost, bool isStructure, int unitCount = 1)
    {
        if (!CanAfford(cost))
        {
            Debug.Log("PlacerManager: sem recrutamento suficiente.");
            return;
        }

        float footprint = placementCheckRadius;
        var sr = prefab.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var size = sr.bounds.size;
            footprint = Mathf.Max(size.x, size.y) * 0.45f;
        }

        if (!ValidatePlacement(worldPos, footprint))
        {
            Debug.Log("PlacerManager: posição inválida/ocupada.");
            return;
        }

        // spend resource immediately
        currentRecruitment -= cost;
        UpdateRecruitmentUI();

        // Spawnar múltiplas unidades com offset
        if (unitCount > 1)
        {
            float offsetDistance = 0.8f; // Distância entre unidades
            for (int i = 0; i < unitCount; i++)
            {
                // Calcula offset em círculo ao redor do ponto clicado
                float angle = (360f / unitCount) * i * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * offsetDistance;
                Vector2 spawnPos = worldPos + offset;
                
                StartCoroutine(BuildCoroutine(prefab, spawnPos, 0, isStructure)); // custo 0 pois já foi descontado
            }
        }
        else
        {
            StartCoroutine(BuildCoroutine(prefab, worldPos, 0, isStructure));
        }
    }

    IEnumerator BuildCoroutine(GameObject prefab, Vector2 worldPos, int cost, bool isStructure)
    {
        float buildTime = defaultBuildTime;
        // create a transparent placeholder (visual)
        GameObject placeholder = new GameObject("building_placeholder");
        var sr = placeholder.AddComponent<SpriteRenderer>();
        var sourceSr = prefab.GetComponent<SpriteRenderer>();
        if (sourceSr != null) sr.sprite = sourceSr.sprite;
        sr.color = new Color(1f, 1f, 1f, 0.45f);
        placeholder.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

        // build progress HUD (world-space)
        GameObject hud = null;
        Image fill = null;
        if (buildProgressPrefab != null)
        {
            hud = Instantiate(buildProgressPrefab, placeholder.transform);
            hud.transform.localPosition = new Vector3(0, sourceSr != null ? sourceSr.bounds.size.y/2 + 0.2f : 0.8f, 0);
            fill = hud.GetComponentInChildren<Image>();
        }

        float t = 0f;
        while (t < buildTime)
        {
            t += Time.deltaTime;
            if (fill != null) fill.fillAmount = Mathf.Clamp01(t / buildTime);
            yield return null;
        }

        // finalize: spawn real prefab under correct container
        GameObject instance = Instantiate(prefab, new Vector3(worldPos.x, worldPos.y, 0f), Quaternion.identity);
        if (isStructure)
        {
            if (structuresContainer != null) instance.transform.SetParent(structuresContainer, true);
        }
        else
        {
            if (unitsContainer != null) instance.transform.SetParent(unitsContainer, true);
        }

        if (hud != null) Destroy(hud);
        Destroy(placeholder);
        UpdateRecruitmentUI();
        // optional: play spawn SFX
    }

    public void RefundPartial(int amount)
    {
        currentRecruitment += amount;
        UpdateRecruitmentUI();
    }

    public void UpdateRecruitmentUI()
    {
        if (recruitmentText != null) recruitmentText.text = $"RECRUTAMENTO: {currentRecruitment}";
    }

    // debug draw
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, placementCheckRadius);
    }
}
