using UnityEngine;

/// <summary>
/// Inimigo Tanque - Unidade pesada com alto HP e alto dano.
/// Move-se lentamente mas é extremamente resistente.
/// Representa forças militares opressoras ou veículos blindados.
/// </summary>
public class EnemyTank : UnitBase
{
    [Header("Tank Settings")]
    private GameObject _targetObjective;
    public GameObject targetObjective 
    { 
        get => _targetObjective;
        set 
        { 
            _targetObjective = value;
            hasObjective = value != null;
        }
    }
    [SerializeField] private bool hasObjective = false;

    [Header("Tank Special")]
    [SerializeField] private float armorReduction = 0.3f; // 30% de redução de dano

    protected override void Awake()
    {
        base.Awake();

        // Define como inimigo
        isEnemy = true;

        // CRITICAL: Garante que tem Collider2D para ser detectado
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.6f; // Tanque um pouco maior
            collider.isTrigger = false;
            Debug.LogWarning($"[EnemyTank] {name} não tinha Collider2D! Adicionado automaticamente.");
        }

        // Stats de tanque: MUITO HP, ALTO dano, LENTO
        unitName = "Tanque Opressor";
        maxHealth = 150f;      // 3.75x mais HP que BasicEnemy
        currentHealth = maxHealth;
        damage = 25f;          // 2.5x mais dano
        attackRange = 2.5f;    // Range ligeiramente maior
        attackCooldown = 2.0f; // Ataque mais lento
        moveSpeed = 1.2f;      // Muito mais lento (45% da velocidade do BasicEnemy)
    }

    protected override void Start()
    {
        base.Start();
        
        // FORÇA o layer correto (ignora configuração do prefab)
        gameObject.layer = LayerMask.NameToLayer("Enemies");
        Debug.Log($"[EnemyTank] {name} FORÇADO para layer Enemies (layer {gameObject.layer})");
        
        // Configura layer enemy
        enemyLayer = LayerMask.GetMask("Units", "Structures");

        // Busca objetivo se não foi definido
        if (targetObjective == null)
        {
            FindObjective();
        }
    }

    protected override void IdleBehavior()
    {
        // Tanques priorizam alvos próximos, mas movem para objetivo se não há combate
        base.IdleBehavior();

        // Se não tem alvo próximo, move em direção ao objetivo
        if (currentTarget == null && targetObjective != null)
        {
            MoveTowardsObjective();
        }
    }

    private void MoveTowardsObjective()
    {
        if (targetObjective == null) return;

        Vector2 direction = (targetObjective.transform.position - transform.position).normalized;
        
        // Usa Rigidbody2D se disponível
        if (rb != null)
        {
            Vector2 newPosition = rb.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
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
        GameObject[] structures = new GameObject[0];
        try
        {
            structures = GameObject.FindGameObjectsWithTag("PlayerCore");
        }
        catch (UnityException)
        {
            Debug.LogWarning("[EnemyTank] Tag 'PlayerCore' não existe!");
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

    // Override TakeDamage para aplicar redução de armadura
    public override void TakeDamage(float damageAmount, UnitBase attacker)
    {
        // Tanque tem armadura: reduz dano recebido
        float reducedDamage = damageAmount * (1f - armorReduction);
        
        Debug.Log($"[EnemyTank] {unitName} recebeu {damageAmount} dano -> {reducedDamage} (armadura: {armorReduction * 100}%)");
        
        base.TakeDamage(reducedDamage, attacker);
    }

    protected override void Die()
    {
        // Tanques podem ter efeito especial de morte
        Debug.Log($"[EnemyTank] {unitName} destruído!");
        
        // TODO: Adicionar explosão ou efeito visual
        
        base.Die();
    }
}
