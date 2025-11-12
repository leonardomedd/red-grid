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

        // Stats específicos do Operário
        unitName = "Comrade Recruit";
        maxHealth = 50f;
        currentHealth = maxHealth;
        damage = 8f;
        attackRange = 2.5f;
        attackCooldown = 1.2f;
        moveSpeed = 2.5f;
    }

    protected override void Start()
    {
        base.Start();
        
        // Configura layer enemy de forma explícita usando o método correto
        enemyLayer = LayerMask.GetMask("Enemies");
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
