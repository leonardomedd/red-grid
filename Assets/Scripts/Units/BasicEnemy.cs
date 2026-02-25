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
            // Debug.Log($"[BasicEnemy] {name} objetivo setado para: {value?.name ?? \"NULL\"}, hasObjective: {hasObjective}");
        }
    }
    [SerializeField] private bool hasObjective = false;

    protected override void Awake()
    {
        base.Awake();

        // Define como inimigo ANTES de qualquer Start
        isEnemy = true;

        // CRITICAL: Garante Rigidbody2D (OBRIGATÓRIO para Physics2D.OverlapCircleAll detectar!)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Sem gravidade para 2D top-down
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.LogWarning($"[BasicEnemy] {name} não tinha Rigidbody2D! Adicionado automaticamente.");
        }
        
        // CRITICAL: Garante que tem Collider2D para ser detectado
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = false; // Colisão física
            Debug.LogWarning($"[BasicEnemy] {name} não tinha Collider2D! Adicionado automaticamente.");
        }
        else
        {
            // Garante que o collider não é trigger
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = false;
        }

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
        
        // FORÇA o layer correto (ignora configuração do prefab)
        gameObject.layer = LayerMask.NameToLayer("Enemies");
        // Debug.Log($"[BasicEnemy] {name} FORÇADO para layer Enemies (layer {gameObject.layer})");
        
        Debug.Log($"[BasicEnemy] {name} SPAWNADO! Posição: {transform.position}, Layer: {gameObject.layer}, isEnemy: {isEnemy}");
        
        // Configura layer enemy de forma explícita usando o método correto
        enemyLayer = LayerMask.GetMask("Units", "Structures");

        // Busca objetivo se não foi definido
        if (targetObjective == null)
        {
            FindObjective();
        }
        
        // Debug.Log($"[BasicEnemy] {name} iniciado. TargetObjective: {targetObjective?.name ?? \"NULL\"}, hasObjective: {hasObjective}");
    }

    protected override void IdleBehavior()
    {
        // Inimigos procuram alvos ou movem em direção ao objetivo
        base.IdleBehavior();

        // Debug (com verificação de null safety)
        if (Time.frameCount % 120 == 0)
        {
            string targetName = currentTarget != null ? currentTarget.name : "NULL";
            string objectiveName = "NULL";
            
            // Unity's destroyed object check: both reference check and Unity's implicit bool check
            if (targetObjective != null && targetObjective)
            {
                objectiveName = targetObjective.name;
            }
            
            // Debug.Log($"[BasicEnemy] {name} IdleBehavior - currentTarget: {targetName}, targetObjective: {objectiveName}");
        }

        // Se não tem alvo próximo, move em direção ao objetivo
        // Verifica se targetObjective existe E não foi destruído
        if (currentTarget == null && targetObjective != null && targetObjective)
        {
            MoveTowardsObjective();
        }
    }

    private void MoveTowardsObjective()
    {
        // Verificação dupla: null e se o objeto ainda existe
        if (targetObjective == null || !targetObjective) return;

        // Safe access to transform
        Transform objTransform = targetObjective.transform;
        if (objTransform == null) return;
        
        // Debug.Log($"[BasicEnemy] {name} MOVENDO em direção a {targetObjective.name}");

        Vector2 direction = (objTransform.position - transform.position).normalized;
        
        // Usa Rigidbody2D se disponível - CORRIGIDO: usar Time.deltaTime
        if (rb != null)
        {
            Vector2 newPosition = rb.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
            // Debug.Log($"[BasicEnemy] Usando Rigidbody2D - de {rb.position} para {newPosition}");
        }
        else
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
            // Debug.Log($"[BasicEnemy] Usando Transform - nova posição: {transform.position}");
        }

        // Flip sprite
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Se chegou perto do objetivo, ataca (verificar se ainda existe)
        if (targetObjective != null && targetObjective)
        {
            float distance = Vector2.Distance(transform.position, targetObjective.transform.position);
            if (distance <= attackRange)
            {
                currentTarget = targetObjective.transform;
                ChangeState(UnitState.Attacking);
            }
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
