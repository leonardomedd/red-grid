using UnityEngine;
using System.Collections;

/// <summary>
/// Sistema de produção automática da fábrica.
/// Gera operários automaticamente a cada intervalo de tempo.
/// Representa a produção constante da classe trabalhadora.
/// </summary>
public class FactoryProduction : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] private GameObject workerPrefab;
    [SerializeField] private float productionInterval = 10f; // Segundos entre cada produção
    [SerializeField] private float spawnRadius = 1.5f; // Raio ao redor da fábrica onde operários spawnam
    [SerializeField] private bool autoStartProduction = true;
    
    [Header("Cost & Resources")]
    [SerializeField] private bool requiresResources = true; // Se consome recursos do PlacerManager
    [SerializeField] private int productionCost = 5; // Custo por operário produzido
    
    [Header("Limits")]
    [SerializeField] private bool hasProductionLimit = false; // Limita produção por wave/tempo
    [SerializeField] private int maxUnitsProduced = -1; // -1 = ilimitado
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer factoryRenderer;
    [SerializeField] private Color producingColor = new Color(1f, 1f, 0f, 0.5f); // Amarelo semi-transparente
    [SerializeField] private GameObject productionEffectPrefab; // Efeito visual opcional
    
    private Color originalColor;
    private PlacerManager placerManager;
    private bool isProducing = false;
    private float productionTimer = 0f;
    private int unitsProducedCount = 0;
    
    private void Start()
    {
        if (requiresResources)
        {
            placerManager = PlacerManager.Instance;
            if (placerManager == null)
            {
                Debug.LogWarning("[FactoryProduction] PlacerManager não encontrado! Desabilitando custo de recursos.");
                requiresResources = false;
            }
        }
        
        if (factoryRenderer != null)
        {
            originalColor = factoryRenderer.color;
        }
        
        if (autoStartProduction)
        {
            StartProduction();
        }
        
        // Debug.Log($"[FactoryProduction] Fábrica inicializada. Intervalo: {productionInterval}s, Custo: {(requiresResources ? productionCost.ToString() : "Grátis")}");
    }
    
    private void Update()
    {
        if (!isProducing)
        {
            Debug.Log($"[FactoryProduction] Update - Produção parada. isProducing = {isProducing}");
            return;
        }
        
        // Verifica se atingiu limite de produção
        if (hasProductionLimit && maxUnitsProduced >= 0 && unitsProducedCount >= maxUnitsProduced)
        {
            StopProduction();
            Debug.Log($"[FactoryProduction] Limite de produção atingido: {unitsProducedCount}/{maxUnitsProduced}");
            return;
        }
        
        productionTimer += Time.deltaTime;
        
        // Log apenas a cada 1 segundo
        // if (Mathf.FloorToInt(productionTimer) != Mathf.FloorToInt(productionTimer - Time.deltaTime))
        // {
        //     Debug.Log($"[FactoryProduction] Timer: {productionTimer:F1}s / {productionInterval}s");
        // }
        
        if (productionTimer >= productionInterval)
        {
            // Debug.Log("[FactoryProduction] Intervalo atingido! Produzindo operário...");
            ProduceWorker();
            productionTimer = 0f;
        }
    }
    
    public void StartProduction()
    {
        isProducing = true;
        productionTimer = 0f;
        // Debug.Log("[FactoryProduction] Produção iniciada!");
    }
    
    public void StopProduction()
    {
        isProducing = false;
        // Debug.Log("[FactoryProduction] Produção parada!");
    }
    
    public void ToggleProduction()
    {
        if (isProducing)
            StopProduction();
        else
            StartProduction();
    }
    
    private void ProduceWorker()
    {
        // Verifica recursos se necessário
        if (requiresResources && placerManager != null)
        {
            if (!placerManager.CanAfford(productionCost))
            {
                Debug.LogWarning($"[FactoryProduction] Recursos insuficientes para produção! Necessário: {productionCost}, Disponível: {placerManager.currentRecruitment}");
                return;
            }
            
            // Gasta recursos
            placerManager.currentRecruitment -= productionCost;
            placerManager.UpdateRecruitmentUI();
            // Debug.Log($"[FactoryProduction] Recursos gastos: {productionCost}. Restante: {placerManager.currentRecruitment}");
        }
        
        if (workerPrefab == null)
        {
            Debug.LogError("[FactoryProduction] Worker prefab não configurado!");
            return;
        }
        
        // Spawna em círculo ao redor da fábrica
        Vector3 spawnPosition = GetRandomSpawnPosition();
        // Debug.Log($"[FactoryProduction] Spawnando operário na posição: {spawnPosition} (Fábrica em: {transform.position})");
        
        // Spawna o operário
        GameObject worker = Instantiate(workerPrefab, spawnPosition, Quaternion.identity);
        // Debug.Log($"[FactoryProduction] Operário criado: {worker.name}");
        
        // Coloca no container de unidades se existir
        if (placerManager != null && placerManager.unitsContainer != null)
        {
            worker.transform.SetParent(placerManager.unitsContainer, true);
            // Debug.Log($"[FactoryProduction] Operário adicionado ao container: {placerManager.unitsContainer.name}");
        }
        
        unitsProducedCount++;
        // Debug.Log($"[FactoryProduction] Operário produzido! Total: {unitsProducedCount}");
        
        // Efeito visual
        StartCoroutine(ProductionFlash());
        
        if (productionEffectPrefab != null)
        {
            GameObject effect = Instantiate(productionEffectPrefab, spawnPosition, Quaternion.identity);
            Destroy(effect, 2f); // Auto-destrói após 2 segundos
        }
    }
    
    private Vector3 GetRandomSpawnPosition()
    {
        // Spawna operários à direita da fábrica (em semicírculo)
        // Ângulo entre -60° e +60° (frente direita)
        float angle = Random.Range(-60f, 60f) * Mathf.Deg2Rad;
        float distance = Random.Range(spawnRadius * 0.8f, spawnRadius * 1.2f); // Mais variação na distância
        
        // Offset base (sempre à direita)
        Vector3 baseOffset = new Vector3(spawnRadius * 1.5f, 0, 0);
        
        // Offset aleatório em semicírculo
        Vector3 randomOffset = new Vector3(
            Mathf.Cos(angle) * distance * 0.5f, // Menor variação horizontal
            Mathf.Sin(angle) * distance, // Mais variação vertical
            0
        );
        
        return transform.position + baseOffset + randomOffset;
    }
    
    private IEnumerator ProductionFlash()
    {
        if (factoryRenderer == null) yield break;
        
        // Flash de cor
        factoryRenderer.color = producingColor;
        yield return new WaitForSeconds(0.3f);
        
        // Volta para cor original
        Color targetColor = originalColor;
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            factoryRenderer.color = Color.Lerp(producingColor, targetColor, elapsed / duration);
            yield return null;
        }
        
        factoryRenderer.color = originalColor;
    }
    
    // Métodos públicos para controle externo e debug
    public float GetProductionProgress()
    {
        if (productionInterval <= 0) return 0;
        return Mathf.Clamp01(productionTimer / productionInterval);
    }
    
    public float GetTimeUntilNextProduction()
    {
        return Mathf.Max(0, productionInterval - productionTimer);
    }
    
    public int GetUnitsProducedCount()
    {
        return unitsProducedCount;
    }
    
    public void ResetProductionCount()
    {
        unitsProducedCount = 0;
        Debug.Log("[FactoryProduction] Contador de produção resetado.");
    }
    
    // Gizmos para visualizar área de spawn
    private void OnDrawGizmosSelected()
    {
        // Desenha círculo verde mostrando área de spawn
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Desenha cruz no centro da fábrica
        Gizmos.color = Color.yellow;
        float crossSize = 0.5f;
        Vector3 pos = transform.position;
        Gizmos.DrawLine(pos + Vector3.left * crossSize, pos + Vector3.right * crossSize);
        Gizmos.DrawLine(pos + Vector3.up * crossSize, pos + Vector3.down * crossSize);
    }
}
