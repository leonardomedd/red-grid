using UnityEngine;

/// <summary>
/// Script temporário para testar combate.
/// Spawna unidades aliadas e inimigas para ver se brigam.
/// </summary>
public class CombatTester : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject comradePrefab;
    public GameObject brigadePrefab;
    public GameObject enemyPrefab;

    [Header("Spawn Settings")]
    public int alliesCount = 3;
    public int enemiesCount = 3;
    public Vector2 alliesSpawnArea = new Vector2(-5, 0);
    public Vector2 enemiesSpawnArea = new Vector2(5, 0);
    public float spawnRadius = 2f;

    void Start()
    {
        SpawnTestUnits();
    }

    [ContextMenu("Spawn Test Units")]
    public void SpawnTestUnits()
    {
        // Spawna aliados
        for (int i = 0; i < alliesCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(
                alliesSpawnArea.x + randomOffset.x,
                alliesSpawnArea.y + randomOffset.y,
                0
            );

            // Alterna entre Comrade e Brigade
            GameObject prefab = (i % 2 == 0) ? comradePrefab : brigadePrefab;
            if (prefab != null)
            {
                Instantiate(prefab, spawnPos, Quaternion.identity);
            }
        }

        // Spawna inimigos
        for (int i = 0; i < enemiesCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = new Vector3(
                enemiesSpawnArea.x + randomOffset.x,
                enemiesSpawnArea.y + randomOffset.y,
                0
            );

            if (enemyPrefab != null)
            {
                Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            }
        }

        Debug.Log($"Spawned {alliesCount} allies and {enemiesCount} enemies for combat test!");
    }

    private void OnDrawGizmos()
    {
        // Desenha áreas de spawn
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(alliesSpawnArea, spawnRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(enemiesSpawnArea, spawnRadius);
    }
}
