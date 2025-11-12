using UnityEngine;

/// <summary>
/// Worker Brigade (Milícia Leve) - Unidade de combate corpo a corpo.
/// Alto HP, curto alcance, dano moderado.
/// </summary>
public class WorkerBrigade : UnitBase
{
    [Header("Worker Brigade Settings")]
    [SerializeField] private float defensiveStance = 0.8f; // Redução de dano quando parado
    [SerializeField] private bool isDefending = false;

    protected override void Awake()
    {
        base.Awake();

        // Define como unidade aliada ANTES de qualquer Start
        isEnemy = false;

        // Stats específicos da Milícia
        unitName = "Worker Brigade";
        maxHealth = 80f;
        currentHealth = maxHealth;
        damage = 15f;
        attackRange = 1.5f; // Corpo a corpo
        attackCooldown = 1.5f;
        moveSpeed = 2.0f; // Mais lento
    }

    protected override void Start()
    {
        base.Start();
        
        // Configura layer enemy de forma explícita usando o método correto
        enemyLayer = LayerMask.GetMask("Enemies");
    }

    protected override void IdleBehavior()
    {
        // Quando idle, entra em postura defensiva
        isDefending = true;
        base.IdleBehavior();
    }

    protected override void MoveBehavior()
    {
        // Quando se move, perde postura defensiva
        isDefending = false;
        base.MoveBehavior();
    }

    protected override void AttackBehavior()
    {
        // Quando ataca, mantém postura defensiva
        isDefending = true;
        base.AttackBehavior();
    }

    public override void TakeDamage(float damageAmount, UnitBase attacker)
    {
        // Aplica redução de dano se estiver defendendo
        if (isDefending)
        {
            damageAmount *= defensiveStance;
            Debug.Log($"{unitName} defendeu! Dano reduzido para {damageAmount}");
        }

        base.TakeDamage(damageAmount, attacker);
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();
        
        // Ataque corpo a corpo pode ter efeito especial
        // TODO: Adicionar screen shake ou efeito de impacto
    }
}
