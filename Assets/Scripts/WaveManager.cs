using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Gerencia o sistema de ondas de inimigos.
/// Spawna inimigos em posições definidas, controla timing entre ondas,
/// e detecta condições de vitória/derrota.
/// </summary>
public class WaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private float timeBetweenWaves = 10f;
    [SerializeField] private float timeBetweenSpawns = 0.5f;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private bool useRandomSpawnPoints = true;

    [Header("Game References")]
    [SerializeField] private GameObject playerCore; // Objetivo que os inimigos atacam
    
    [Header("Debug")]
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private int currentWaveIndex = 0;

    // Estado do sistema
    private WaveState currentState = WaveState.Waiting;
    private int enemiesAlive = 0;
    private float waveTimer = 0f;
    private bool gameEnded = false;

    // Eventos
    public event Action<int> OnWaveStart; // (waveNumber)
    public event Action<int> OnWaveComplete; // (waveNumber)
    public event Action OnAllWavesComplete;
    public event Action OnGameOver;

    private enum WaveState
    {
        Waiting,        // Aguardando iniciar próxima wave
        Spawning,       // Spawnando inimigos
        Fighting,       // Inimigos ativos no campo
        Complete        // Todas as waves completadas
    }

    void Start()
    {
        // Valida spawn points
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[WaveManager] Nenhum spawn point configurado!");
            return;
        }

        // Encontra o core automaticamente se não configurado
        if (playerCore == null)
        {
            GameObject coreObj = GameObject.FindGameObjectWithTag("PlayerCore");
            if (coreObj != null)
            {
                playerCore = coreObj;
                // Debug.Log("[WaveManager] Player Core encontrado automaticamente");
            }
            else
            {
                Debug.LogWarning("[WaveManager] Player Core não encontrado! Configure manualmente ou adicione tag 'PlayerCore'");
            }
        }

        if (autoStartWaves)
        {
            StartWaveSystem();
        }
    }

    void Update()
    {
        if (gameEnded) return;

        // Verifica se o core foi destruído
        if (playerCore == null)
        {
            TriggerGameOver();
            return;
        }

        // Máquina de estados
        switch (currentState)
        {
            case WaveState.Waiting:
                UpdateWaitingState();
                break;
            case WaveState.Fighting:
                UpdateFightingState();
                break;
        }
    }

    private void UpdateWaitingState()
    {
        waveTimer += Time.deltaTime;

        if (waveTimer >= timeBetweenWaves)
        {
            StartNextWave();
        }
    }

    private void UpdateFightingState()
    {
        // Verifica se todos os inimigos foram derrotados
        // Atualiza a cada segundo para não sobrecarregar
        if (Time.frameCount % 60 == 0)
        {
            UpdateEnemyCount();
        }

        if (enemiesAlive <= 0)
        {
            // Verificação final antes de completar
            UpdateEnemyCount();
            
            if (enemiesAlive <= 0)
            {
                // Debug.Log("[WaveManager] Todos os inimigos eliminados! Completando wave...");
                CompleteCurrentWave();
            }
        }
    }

    public void StartWaveSystem()
    {
        if (waves.Count == 0)
        {
            Debug.LogError("[WaveManager] Nenhuma wave configurada!");
            return;
        }

        currentWaveIndex = 0;
        currentState = WaveState.Waiting;
        waveTimer = 0f;
        
        // Debug.Log($"[WaveManager] Sistema de waves iniciado. Total: {waves.Count} waves");
    }

    private void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            // Todas as waves completadas
            CompleteAllWaves();
            return;
        }

        currentState = WaveState.Spawning;
        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        // Debug.Log($"[WaveManager] ========== Iniciando Wave {currentWaveIndex + 1}/{waves.Count}: {wave.waveName} ==========");
        OnWaveStart?.Invoke(currentWaveIndex + 1);

        int totalToSpawn = 0;
        foreach (EnemySpawnData spawnData in wave.enemies)
        {
            totalToSpawn += spawnData.count;
        }
        // Debug.Log($"[WaveManager] Total de inimigos planejados para spawn: {totalToSpawn}");

        // Spawna cada grupo de inimigos
        int spawnedCount = 0;
        foreach (EnemySpawnData spawnData in wave.enemies)
        {
            for (int i = 0; i < spawnData.count; i++)
            {
                SpawnEnemy(spawnData.enemyPrefab);
                spawnedCount++;
                // Debug.Log($"[WaveManager] Spawnado {spawnedCount}/{totalToSpawn}");
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        // Todos os inimigos foram spawnados
        currentState = WaveState.Fighting;
        // Debug.Log($"[WaveManager] ========== Wave {currentWaveIndex + 1} spawnada completamente. Total enemiesAlive: {enemiesAlive} ==========");
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("[WaveManager] Enemy prefab é null!");
            return;
        }

        // Seleciona spawn point
        Transform spawnPoint = GetSpawnPoint();
        Vector3 spawnPosition = spawnPoint.position;

        // Adiciona um offset aleatório pequeno para não spawnar todos no mesmo lugar
        spawnPosition += new Vector3(
            UnityEngine.Random.Range(-0.5f, 0.5f),
            UnityEngine.Random.Range(-0.5f, 0.5f),
            0
        );

        // Spawna o inimigo
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        // Debug.Log($"[WaveManager] Instanciado: {enemy.name} em {spawnPosition}");
        
        // Configura o objetivo (core do jogador)
        BasicEnemy enemyScript = enemy.GetComponent<BasicEnemy>();
        if (enemyScript != null && playerCore != null)
        {
            enemyScript.targetObjective = playerCore;
            // Debug.Log($"[WaveManager] → Configurado para atacar {playerCore.name}");
        }
        else
        {
            if (enemyScript == null)
                Debug.LogError("[WaveManager] → Enemy não tem componente BasicEnemy!");
            if (playerCore == null)
                Debug.LogError("[WaveManager] → PlayerCore é null!");
        }

        // Subscreve ao evento de morte ANTES de incrementar
        UnitBase unitBase = enemy.GetComponent<UnitBase>();
        if (unitBase != null)
        {
            unitBase.OnDeath += OnEnemyDied;
            // Debug.Log($"[WaveManager] → Inscrito no evento OnDeath de {enemy.name}");
        }
        else
        {
            Debug.LogError($"[WaveManager] → {enemy.name} não tem UnitBase! Não será rastreado!");
        }

        // Incrementa contador
        enemiesAlive++;
        // Debug.Log($"[WaveManager] → enemiesAlive agora é {enemiesAlive}");
    }

    private Transform GetSpawnPoint()
    {
        if (useRandomSpawnPoints)
        {
            return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        }
        else
        {
            // Usa spawn points em ordem circular
            return spawnPoints[enemiesAlive % spawnPoints.Length];
        }
    }

    private void OnEnemyDied(UnitBase unit)
    {
        // Null safety
        if (unit == null) return;
        
        // Desinscreve do evento para evitar chamadas duplicadas
        unit.OnDeath -= OnEnemyDied;
        
        // Decrementa com proteção contra valores negativos
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        
        // Debug.Log($"[WaveManager] Inimigo morreu: {unit.name}. Restantes: {enemiesAlive}");
        
        // Força atualização quando está quase vazio para garantir consistência
        if (enemiesAlive <= 1)
        {
            // Debug.Log("[WaveManager] LAST ENEMY - forçando UpdateEnemyCount()");
            UpdateEnemyCount();
        }
    }

    private void UpdateEnemyCount()
    {
        // Conta inimigos vivos manualmente (fallback se eventos falharem)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemies");
        int count = 0;
        
        // Debug.Log($"[WaveManager] UpdateEnemyCount - Total GameObjects com tag 'Enemies': {enemies.Length}, enemiesAlive atual: {enemiesAlive}");

        foreach (GameObject enemy in enemies)
        {
            // Verificação robusta: null e se o objeto ainda existe
            if (enemy == null || !enemy)
            {
                // Debug.Log("[WaveManager] GameObject nulo ou destruído encontrado");
                continue;
            }
            
            UnitBase unit = enemy.GetComponent<UnitBase>();
            if (unit == null)
            {
                Debug.LogWarning($"[WaveManager] GameObject {enemy.name} tem tag 'Enemies' mas sem UnitBase!");
                continue;
            }
            
            if (!unit.isDead)
            {
                count++;
                // Debug.Log($"[WaveManager] ✓ VIVO: {enemy.name} (isDead={unit.isDead})");
            }
            else
            {
                // Debug.Log($"[WaveManager] ✗ MORTO: {enemy.name} (isDead={unit.isDead}, aguardando destruição)");
            }
        }
        
        // Atualiza apenas se houver diferença significativa (evita bugs de sincronia)
        if (count != enemiesAlive)
        {
            // Debug.Log($"[WaveManager] Correção de contagem: {enemiesAlive} -> {count} inimigos vivos");
            enemiesAlive = count;
        }
    }

    private void CompleteCurrentWave()
    {
        // Debug.Log($"[WaveManager] Wave {currentWaveIndex + 1} completada!");
        OnWaveComplete?.Invoke(currentWaveIndex + 1);

        currentWaveIndex++;
        currentState = WaveState.Waiting;
        waveTimer = 0f;
    }

    private void CompleteAllWaves()
    {
        // Debug.Log("[WaveManager] Todas as waves completadas! VITÓRIA!");
        currentState = WaveState.Complete;
        gameEnded = true;
        OnAllWavesComplete?.Invoke();
    }

    private void TriggerGameOver()
    {
        if (gameEnded) return;

        // Debug.Log("[WaveManager] Core destruído! DERROTA!");
        gameEnded = true;
        currentState = WaveState.Complete;
        OnGameOver?.Invoke();
    }

    // Métodos públicos para controle manual
    public void ForceStartNextWave()
    {
        if (currentState == WaveState.Waiting)
        {
            waveTimer = timeBetweenWaves;
        }
    }

    public int GetCurrentWave() => currentWaveIndex + 1;
    public int GetTotalWaves() => waves.Count;
    public float GetTimeUntilNextWave() => Mathf.Max(0, timeBetweenWaves - waveTimer);
    public int GetEnemiesAlive() => enemiesAlive;
    public bool IsGameEnded() => gameEnded;

    // Gizmos para visualizar spawn points
    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up * 2f);
            }
        }
    }
}

/// <summary>
/// Define uma onda de inimigos.
/// </summary>
[System.Serializable]
public struct Wave
{
    public string waveName;
    public List<EnemySpawnData> enemies;
}

/// <summary>
/// Define um tipo de inimigo e quantos spawnar.
/// </summary>
[System.Serializable]
public struct EnemySpawnData
{
    public GameObject enemyPrefab;
    public int count;
}
