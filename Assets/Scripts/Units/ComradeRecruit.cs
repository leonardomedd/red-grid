using UnityEngine;

/// <summary>
/// Comrade Recruit - Unidade básica de infantaria.
/// Baixo custo, stats balanceados, versátil.
/// </summary>
public class ComradeRecruit : UnitBase
{
    [Header("Comrade Recruit Settings")]
    [SerializeField] private float moralBonus = 1f; // Bonus de moral quando sobrevive

    protected override void Awake()
    {
        base.Awake();

        // Define como unidade aliada ANTES de qualquer Start
        isEnemy = false;
        
        // CRITICAL: Garante que tem Collider2D
        // CRITICAL: Garante Rigidbody2D (OBRIGATÓRIO para Physics2D.OverlapCircleAll detectar!)
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Sem gravidade para 2D top-down
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = false;
        }

        // Stats específicos do Operário
        unitName = "Comrade Recruit";
        maxHealth = 50f;
        currentHealth = maxHealth;
        damage = 8f;
        attackRange = 1.5f;  // Alcance de ataque melee
        attackCooldown = 1.2f;
        moveSpeed = 3.5f; // AUMENTADO para se mover mais rápido em direção aos inimigos
    }

    protected override void Start()
    {
        base.Start();
        
        // Configura layer enemy de forma explícita usando o método correto
        enemyLayer = LayerMask.GetMask("Enemies");
        
        Debug.Log($"[ComradeRecruit] {name} iniciado! isEnemy={isEnemy}, layer={gameObject.layer}, attackRange={attackRange}, Posição: {transform.position}");
    }

    // Comportamento especial: ao sobreviver uma onda, gera moral
    public void OnWaveSurvived()
    {
        // TODO: Integrar com sistema de Moral quando implementado
        Debug.Log($"{unitName} sobreviveu à onda! +{moralBonus} Moral");
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Efeito sonoro/visual específico (placeholder)
        // TODO: Adicionar SFX de tiro/ataque
    }
}
