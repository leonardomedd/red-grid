using UnityEngine;

/// <summary>
/// Inimigo básico - Representa forças reacionárias/burguesas.
/// Usado para testar o sistema de combate.
/// </summary>
public class BasicEnemy : UnitBase
{
    [Header("Enemy Settings")]
    private GameObject _targetObjective; // Objetivo a ser atacado (ex: base)
    public GameObject targetObjective 
    { 
        get => _targetObjective;
        set 
        { 
            _targetObjective = value;
            hasObjective = value != null;
            Debug.Log($"[BasicEnemy] {name} objetivo setado para: {value?.name ?? "NULL"}, hasObjective: {hasObjective}");
        }
    }
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
        
        Debug.Log($"[BasicEnemy] {name} iniciado. TargetObjective: {targetObjective?.name ?? "NULL"}, hasObjective: {hasObjective}");
    }

    protected override void IdleBehavior()
    {
        // Inimigos procuram alvos ou movem em direção ao objetivo
        base.IdleBehavior();

        // Debug
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"[BasicEnemy] {name} IdleBehavior - currentTarget: {currentTarget?.name ?? "NULL"}, targetObjective: {targetObjective?.name ?? "NULL"}");
        }

        // Se não tem alvo próximo, move em direção ao objetivo
        // Verifica se targetObjective existe (mesmo que hasObjective seja false)
        if (currentTarget == null && targetObjective != null)
        {
            MoveTowardsObjective();
        }
    }

    private void MoveTowardsObjective()
    {
        if (targetObjective == null) return;

        Debug.Log($"[BasicEnemy] {name} MOVENDO em direção a {targetObjective.name}");

        Vector2 direction = (targetObjective.transform.position - transform.position).normalized;
        
        // Usa Rigidbody2D se disponível - CORRIGIDO: usar Time.deltaTime
        if (rb != null)
        {
            Vector2 newPosition = rb.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
            Debug.Log($"[BasicEnemy] Usando Rigidbody2D - de {rb.position} para {newPosition}");
        }
        else
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            Debug.Log($"[BasicEnemy] Usando Transform - nova posição: {transform.position}");
        }

        // Flip sprite
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Se chegou perto do objetivo, ataca
        float distance = Vector2.Distance(transform.position, targetObjective.transform.position);
        if (distance <= attackRange)
        {
            currentTarget = targetObjective.transform;
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
            GameObject closest = null;
            float closestDist = Mathf.Infinity;

            foreach (var structure in structures)
            {
                float dist = Vector2.Distance(transform.position, structure.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = structure;
                }
            }

            targetObjective = closest;
            hasObjective = true;
        }
        else
        {
            // Fallback: move em direção ao centro do mapa
            GameObject tempObj = new GameObject("TempObjective");
            tempObj.transform.position = Vector3.zero;
            targetObjective = tempObj;
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
