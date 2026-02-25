using UnityEngine;

public class UnitCardUI : MonoBehaviour
{
    [Header("Card data")]
    public GameObject ghostPrefab;  // prefab ghost visual
    public GameObject unitPrefab;   // prefab real to spawn
    public int cost = 1;
    public bool isStructure = false;
    [Header("Multiple Units")]
    [Tooltip("Quantidade de unidades a spawnar (ex: 3 para operários)")]
    public int unitsPerPlacement = 1;

    private GameObject currentGhost;

    // Called by Button.onClick or UI event
    public void OnPickCard()
    {
        if (currentGhost != null) return;
        if (PlacerManager.Instance == null)
        {
            Debug.LogWarning("PlacerManager instance not found.");
            return;
        }
        if (!PlacerManager.Instance.CanAfford(cost))
        {
            Debug.Log("Não há recrutamento suficiente para escolher esta carta.");
            return;
        }

        currentGhost = Instantiate(ghostPrefab);
        GhostFollower gf = currentGhost.GetComponent<GhostFollower>();
        if (gf != null)
        {
            gf.Init(this, unitPrefab, cost, isStructure, unitsPerPlacement);
        }
        else
        {
            Debug.LogWarning("Ghost prefab não tem GhostFollower.");
        }
    }

    // Método público esperado por GhostFollower — CANCELA a colocação
    public void CancelPlacement()
    {
        if (currentGhost != null)
        {
            Destroy(currentGhost);
            currentGhost = null;
        }
    }
}
