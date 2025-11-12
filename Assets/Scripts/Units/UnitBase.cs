using UnityEngine;
using System.Collections;

/// <summary>
/// Classe base para todas as unidades do jogo.
/// Define stats, estados e comportamentos comuns.
/// </summary>
public abstract class UnitBase : MonoBehaviour
{
    [Header("Unit Stats")]
    [SerializeField] protected string unitName = "Unit";
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attackRange = 3f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float moveSpeed = 2f;

    [Header("Combat")]
    [SerializeField] protected LayerMask enemyLayer;
    [SerializeField] protected bool isEnemy = false; // false = aliado, true = inimigo

    [Header("Visual Feedback")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color hurtColor = Color.red;
    [SerializeField] protected float hurtFlashDuration = 0.1f;

    // Estado interno
    protected UnitState currentState = UnitState.Idle;
    protected Transform currentTarget;
    protected float lastAttackTime;
    protected bool isDead = false;

    // Eventos (para UI, sons, etc)
    public delegate void UnitEvent(UnitBase unit);
    public event UnitEvent OnDeath;
    public event UnitEvent OnDamageTaken;
    public event UnitEvent OnAttack;

    // Estados possíveis
    protected enum UnitState
    {
        Idle,       // Parado esperando
        Moving,     // Movendo em direção ao alvo
        Attacking,  // Atacando
        Dead        // Morto
    }

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // Só sobrescreve layer se ainda estiver no Default
        // (permite configurar manualmente no Inspector)
        if (gameObject.layer == 0) // Default layer
        {
            if (isEnemy)
                gameObject.layer = LayerMask.NameToLayer("Enemies");
            else
                gameObject.layer = LayerMask.NameToLayer("Units");
                
            layerName = LayerMask.LayerToName(gameObject.layer);
            Debug.Log($"[{unitName}] Layer alterado para: {gameObject.layer} ({layerName})");
        }
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // Máquina de estados simples
        switch (currentState)
        {
            case UnitState.Idle:
                IdleBehavior();
                break;
            case UnitState.Moving:
                MoveBehavior();
                break;
            case UnitState.Attacking:
                AttackBehavior();
                break;
        }
    }

    #region Estado: Idle
    protected virtual void IdleBehavior()
    {
        // Procura inimigos no alcance
        FindTarget();

        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            
            if (distance <= attackRange)
            {
                ChangeState(UnitState.Attacking);
            }
            else
            {
                ChangeState(UnitState.Moving);
            }
        }
    }
    #endregion

    #region Estado: Moving
    protected virtual void MoveBehavior()
    {
        if (currentTarget == null)
        {
            ChangeState(UnitState.Idle);
            return;
        }

        // Move em direção ao alvo
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        // Flip sprite baseado na direção
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Checa se chegou no alcance de ataque
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            ChangeState(UnitState.Attacking);
        }
    }
    #endregion

    #region Estado: Attacking
    protected virtual void AttackBehavior()
    {
        if (currentTarget == null)
        {
            ChangeState(UnitState.Idle);
            return;
        }

        // Verifica se ainda está no alcance
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        if (distance > attackRange)
        {
            ChangeState(UnitState.Moving);
            return;
        }

        // Sistema de cooldown de ataque
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    protected virtual void PerformAttack()
    {
        if (currentTarget == null) return;

        // Aplica dano ao alvo
        UnitBase targetUnit = currentTarget.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(damage, this);
            OnAttack?.Invoke(this);
            
            // Debug visual
            Debug.DrawLine(transform.position, currentTarget.position, Color.red, 0.2f);
        }
    }
    #endregion

    #region Detecção de Alvos
    protected virtual void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange * 1.5f, enemyLayer);

        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            // Ignora a si mesmo
            if (hit.gameObject == gameObject) continue;

            // Verifica se é inimigo válido
            UnitBase unit = hit.GetComponent<UnitBase>();
            if (unit != null && unit.isEnemy != this.isEnemy && !unit.isDead)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = hit.transform;
                }
            }
        }

        currentTarget = closestEnemy;
        
        if (currentTarget != null && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[{unitName}] ✓ Alvo encontrado: {currentTarget.name} a {closestDistance:F2}m");
        }
    }
    #endregion

    #region Dano e Morte
    public virtual void TakeDamage(float damageAmount, UnitBase attacker)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        OnDamageTaken?.Invoke(this);

        // Feedback visual de dano
        StartCoroutine(FlashHurt());

        // Debug
        Debug.Log($"{unitName} recebeu {damageAmount} de dano. HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = UnitState.Dead;
        
        OnDeath?.Invoke(this);

        Debug.Log($"{unitName} morreu!");

        // Animação/efeito de morte (placeholder)
        StartCoroutine(DeathSequence());
    }

    protected virtual IEnumerator DeathSequence()
    {
        // Fade out simples
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                color.a = Mathf.Lerp(1f, 0f, elapsed / duration);
                spriteRenderer.color = color;
                yield return null;
            }
        }

        Destroy(gameObject);
    }

    protected virtual IEnumerator FlashHurt()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hurtColor;
            yield return new WaitForSeconds(hurtFlashDuration);
            spriteRenderer.color = normalColor;
        }
    }
    #endregion

    #region Utilidades
    protected virtual void ChangeState(UnitState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    public bool IsDead() => isDead;
    public float GetHealthPercent() => currentHealth / maxHealth;
    public string GetUnitName() => unitName;
    #endregion

    #region Debug
    private void OnDrawGizmosSelected()
    {
        // Desenha alcance de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Desenha alcance de detecção
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange * 1.5f);

        // Linha para o alvo atual
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
    #endregion
}
