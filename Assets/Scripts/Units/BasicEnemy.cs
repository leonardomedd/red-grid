using UnityEngine;

/// <summary>
/// Inimigo básico - Representa forças reacionárias/burguesas.
/// Usado para testar o sistema de combate.
/// </summary>
public class BasicEnemy : UnitBase
{
    [Header("Enemy Settings")]
    [SerializeField] private Transform targetObjective; // Objetivo a ser atacado (ex: base)
    [SerializeField] private bool hasObjective = false;

    protected override void Awake()
    {
        base.Awake();

        // Define como inimigo ANTES de qualquer Start
        isEnemy = true;

        // Stats de inimigo básico
        unitName = "Reacionário";
        maxHealth = 40f;
        currentHealth = maxHealth;
        damage = 10f;
        attackRange = 2.0f;
        attackCooldown = 1.3f;
        moveSpeed = 2.2f;
    }

    protected override void Start()
    {
        base.Start();
        
        // Configura layer enemy de forma explícita usando o método correto
        enemyLayer = LayerMask.GetMask("Units", "Structures");

        // Busca objetivo se não foi definido
        if (targetObjective == null)
        {
            FindObjective();
        }
    }

    protected override void IdleBehavior()
    {
        // Inimigos procuram alvos ou movem em direção ao objetivo
        base.IdleBehavior();

        // Se não tem alvo próximo, move em direção ao objetivo
        if (currentTarget == null && hasObjective && targetObjective != null)
        {
            MoveTowardsObjective();
        }
    }

    private void MoveTowardsObjective()
    {
        if (targetObjective == null) return;

        Vector2 direction = (targetObjective.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        // Flip sprite
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Se chegou perto do objetivo, ataca
        float distance = Vector2.Distance(transform.position, targetObjective.position);
        if (distance <= attackRange)
        {
            currentTarget = targetObjective;
            ChangeState(UnitState.Attacking);
        }
    }

    private void FindObjective()
    {
        // Procura estruturas inimigas como objetivo
        // TODO: Implementar sistema de objetivos (core, estruturas importantes)
        
        // Verifica se a tag existe antes de usar
        GameObject[] structures = new GameObject[0];
        try
        {
            structures = GameObject.FindGameObjectsWithTag("Structures");
        }
        catch (UnityException)
        {
            Debug.LogWarning("Tag 'Structures' não existe. Crie em Edit > Project Settings > Tags and Layers");
        }
        
        if (structures.Length > 0)
        {
            // Pega estrutura mais próxima
            Transform closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var structure in structures)
            {
                float dist = Vector2.Distance(transform.position, structure.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = structure.transform;
                }
            }

            targetObjective = closest;
            hasObjective = true;
        }
        else
        {
            // Fallback: move em direção ao centro do mapa
            targetObjective = new GameObject("TempObjective").transform;
            targetObjective.position = Vector3.zero;
            hasObjective = true;
        }
    }

    protected override void Die()
    {
        // Inimigos podem dropar recursos ou dar recompensas
        // TODO: Integrar com sistema de recursos
        
        base.Die();
    }
}
