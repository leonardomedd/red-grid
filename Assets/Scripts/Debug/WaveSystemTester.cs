using UnityEngine;

/// <summary>
/// Testa o sistema de waves com configuração rápida de 3 ondas.
/// Adicione este script a um GameObject com WaveManager.
/// </summary>
public class WaveSystemTester : MonoBehaviour
{
    [Header("Wave Configuration")]
    [SerializeField] private GameObject basicEnemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerCore;

    [Header("Auto Setup")]
    [SerializeField] private bool autoSetupWaves = true;

    private WaveManager waveManager;

    void Start()
    {
        waveManager = GetComponent<WaveManager>();
        
        if (waveManager == null)
        {
            Debug.LogError("[WaveSystemTester] WaveManager não encontrado neste GameObject!");
            return;
        }

        if (autoSetupWaves)
        {
            SetupTestWaves();
        }
    }

    [ContextMenu("Setup Test Waves")]
    public void SetupTestWaves()
    {
        Debug.Log("[WaveSystemTester] Configurando waves de teste...");

        // Valida prefab
        if (basicEnemyPrefab == null)
        {
            Debug.LogError("[WaveSystemTester] basicEnemyPrefab não configurado!");
            return;
        }

        // Cria spawn points automaticamente se não existirem
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }

        // Cria player core automaticamente se não existir
        if (playerCore == null)
        {
            CreatePlayerCore();
        }

        Debug.Log("[WaveSystemTester] Waves configuradas! Use WaveManager.StartWaveSystem() para iniciar.");
    }

    private void CreateDefaultSpawnPoints()
    {
        Debug.Log("[WaveSystemTester] Criando spawn points padrão...");

        GameObject spawnParent = new GameObject("SpawnPoints");
        spawnPoints = new Transform[4];

        // 4 spawn points nos cantos
        Vector3[] positions = new Vector3[]
        {
            new Vector3(10, 10, 0),   // Top Right
            new Vector3(-10, 10, 0),  // Top Left
            new Vector3(10, -10, 0),  // Bottom Right
            new Vector3(-10, -10, 0)  // Bottom Left
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject spawn = new GameObject($"SpawnPoint_{i + 1}");
            spawn.transform.parent = spawnParent.transform;
            spawn.transform.position = positions[i];
            spawnPoints[i] = spawn.transform;

            // Adiciona marcador visual
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.parent = spawn.transform;
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.5f;
            marker.GetComponent<Renderer>().material.color = Color.red;
            Destroy(marker.GetComponent<Collider>()); // Remove collider
        }

        Debug.Log($"[WaveSystemTester] {spawnPoints.Length} spawn points criados");
    }

    private void CreatePlayerCore()
    {
        Debug.Log("[WaveSystemTester] Criando Player Core...");

        // Cria GameObject vazio ao invés de primitiva 3D
        playerCore = new GameObject("PlayerCore");
        playerCore.transform.position = Vector3.zero;
        
        // Verifica se a tag existe
        try
        {
            playerCore.tag = "PlayerCore";
        }
        catch (UnityException)
        {
            Debug.LogError("[WaveSystemTester] Tag 'PlayerCore' não existe! Crie em Edit > Project Settings > Tags and Layers");
            return;
        }

        // Adiciona SpriteRenderer para visualização 2D
        SpriteRenderer spriteRenderer = playerCore.AddComponent<SpriteRenderer>();
        spriteRenderer.color = Color.blue;
        
        // Cria um sprite quadrado simples
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
        spriteRenderer.transform.localScale = Vector3.one * 2f;

        // Adiciona collider 2D
        BoxCollider2D collider = playerCore.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one * 2f;

        // Adiciona script de PlayerCore
        playerCore.AddComponent<PlayerCore>();
        
        Debug.Log("[WaveSystemTester] Player Core criado em (0,0,0)");
    }

    // Visualização dos spawn points
    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform spawn in spawnPoints)
        {
            if (spawn != null)
            {
                Gizmos.DrawWireSphere(spawn.position, 1f);
            }
        }

        // Desenha linha dos spawns até o core
        if (playerCore != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Transform spawn in spawnPoints)
            {
                if (spawn != null)
                {
                    Gizmos.DrawLine(spawn.position, playerCore.transform.position);
                }
            }
        }
    }
}
