using UnityEngine;
using System.Collections;

/// <summary>
/// Tipos de priorização de alvos disponíveis
/// </summary>
public enum TargetPriority
{
    Closest,        // Alvo mais próximo (padrão)
    LowestHealth,   // Alvo mais fraco (menor HP)
    HighestDamage   // Maior ameaça (maior dano)
}

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
    
    [Header("Target Priority")]
    [SerializeField] protected TargetPriority targetPriority = TargetPriority.Closest;

    [Header("Visual Feedback")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Color normalColor = Color.white;
    [SerializeField] protected Color hurtColor = Color.red;
    [SerializeField] protected float hurtFlashDuration = 0.1f;

    // Estado interno
    protected UnitState currentState = UnitState.Idle;
    protected Transform currentTarget;
    protected float lastAttackTime;
    public bool isDead = false; // Público para WaveManager verificar
    protected Rigidbody2D rb; // Para movimento com física

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
        
        // Pega Rigidbody2D se existir
        rb = GetComponent<Rigidbody2D>();
        
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        // Só sobrescreve layer se ainda estiver no Default
        // (permite configurar manualmente no Inspector)
        if (gameObject.layer == 0) // Default layer
        {
            if (isEnemy)
            {
                gameObject.layer = LayerMask.NameToLayer("Enemies");
                // Debug.Log($"[{unitName}] Configurado para layer Enemies (layer {gameObject.layer})");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Units");
                // Debug.Log($"[{unitName}] Configurado para layer Units (layer {gameObject.layer})");
            }
        }
        else
        {
            // Debug.Log($"[{unitName}] Layer já configurado: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");
        }
        
        // Debug.Log($"[{unitName}] isEnemy={isEnemy}, enemyLayer mask={enemyLayer.value}");
    }

    protected virtual void Update()
    {
        if (isDead) return;

        // Debug a cada 2 segundos
        if (Time.frameCount % 120 == 0)
        {
            // Debug.Log($"[{unitName}] Estado: {currentState}, Target: {currentTarget?.name ?? \"NULL\"}, Position: {transform.position}");
        }

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
            
            // Log temporário para debug
            if (!isEnemy && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[{unitName}] TARGET ENCONTRADO: {currentTarget.name}, Distância: {distance:F2}, AttackRange: {attackRange}");
            }
            
            if (distance <= attackRange)
            {
                ChangeState(UnitState.Attacking);
            }
            else
            {
                ChangeState(UnitState.Moving);
            }
        }
        else if (!isEnemy) // Aliados patrulham em direção ao centro quando não têm alvos
        {
            // Log temporário
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"[{unitName}] SEM ALVO - Patrulhando para centro. Posição: {transform.position}");
            }
            
            Vector2 centerPosition = Vector2.zero;
            float distanceToCenter = Vector2.Distance(transform.position, centerPosition);
            
            if (distanceToCenter > 3f)
            {
                Vector2 direction = (centerPosition - (Vector2)transform.position).normalized;
                
                if (rb != null)
                {
                    Vector2 newPosition = rb.position + direction * moveSpeed * Time.deltaTime * 0.5f;
                    rb.MovePosition(newPosition);
                }
                else
                {
                    transform.position += (Vector3)direction * moveSpeed * Time.deltaTime * 0.5f;
                }
                
                if (spriteRenderer != null && direction.x != 0)
                {
                    spriteRenderer.flipX = direction.x < 0;
                }
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
        
        // Usa Rigidbody2D se disponível - CORRIGIDO: usar Time.deltaTime
        if (rb != null)
        {
            Vector2 newPosition = rb.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
        }
        else
        {
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }

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

        // Tenta atacar unidade
        UnitBase targetUnit = currentTarget.GetComponent<UnitBase>();
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(damage, this);
            OnAttack?.Invoke(this);
            
            // Debug visual
            Debug.DrawLine(transform.position, currentTarget.position, Color.red, 0.2f);
            return;
        }

        // Tenta atacar Player Core
        PlayerCore playerCore = currentTarget.GetComponent<PlayerCore>();
        if (playerCore != null)
        {
            playerCore.TakeDamage(damage);
            OnAttack?.Invoke(this);
            
            // Debug visual
            Debug.DrawLine(transform.position, currentTarget.position, Color.red, 0.2f);
            return;
        }
    }
    #endregion

    #region Detecção de Alvos
    protected virtual void FindTarget()
    {
        // WORKAROUND: Ignora layer mask porque a Physics Matrix está bloqueando
        // Procura TODOS os colliders e filtra manualmente
        // AUMENTADO: Usa raio MUITO maior para aliados detectarem inimigos distantes
        float searchRadius = isEnemy ? (attackRange * 1.5f) : (attackRange * 10f); // Aliados buscam 10x mais longe
        Collider2D[] allHits = Physics2D.OverlapCircleAll(transform.position, searchRadius);

        // DEBUG CRÍTICO: Log SEMPRE para operários mostrando TUDO que foi detectado
        if (!isEnemy && Time.frameCount % 120 == 0)
        {
            Debug.Log($"[{unitName}] FindTarget - Posição: {transform.position}, Raio: {searchRadius:F2}, Objetos detectados: {allHits.Length}");
            
            // Mostra TODOS os colliders detectados (mesmo sem UnitBase)
            foreach (var hit in allHits)
            {
                if (hit == null) 
                {
                    Debug.LogWarning($"[{unitName}] → Collider NULL detectado!");
                    continue;
                }
                
                if (hit.gameObject == gameObject) continue;
                
                UnitBase unit = hit.GetComponent<UnitBase>();
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                
                if (unit != null)
                {
                    Debug.Log($"[{unitName}] → {hit.name} COM UnitBase: isEnemy={unit.isEnemy}, isDead={unit.isDead}, dist={dist:F2}, layer={hit.gameObject.layer}");
                }
                else
                {
                    Debug.Log($"[{unitName}] → {hit.name} SEM UnitBase: layer={hit.gameObject.layer}, dist={dist:F2}");
                }
            }
        }

        Transform bestTarget = null;
        float bestValue = Mathf.Infinity;

        foreach (var hit in allHits)
        {
            // Ignora a si mesmo
            if (hit.gameObject == gameObject) continue;

            // Verifica se é inimigo válido
            UnitBase unit = hit.GetComponent<UnitBase>();
            if (unit != null && unit.isEnemy != this.isEnemy && !unit.isDead)
            {
                float value = CalculateTargetPriority(unit);
                
                if (value < bestValue)
                {
                    bestValue = value;
                    bestTarget = hit.transform;
                }
                
                // Debug: alvo válido encontrado
                if (Time.frameCount % 120 == 0)
                {
                    // Debug.Log($"[{unitName}] → Alvo válido: {hit.name}, Distância: {Vector2.Distance(transform.position, hit.transform.position):F2}");
                }
            }
            // else if (Time.frameCount % 120 == 0 && currentTarget == null)
            // {
            //     // Debug: por que não é válido?
            //     if (unit == null)
            //         Debug.Log($"[{unitName}] → {hit.name} ignorado: sem UnitBase");
            //     else if (unit.isEnemy == this.isEnemy)
            //         Debug.Log($"[{unitName}] → {hit.name} ignorado: mesmo time (ambos {(isEnemy ? \"inimigos\" : \"aliados\")})");
            //     else if (unit.isDead)
            //         Debug.Log($"[{unitName}] → {hit.name} ignorado: já está morto");
            // }
        }

        currentTarget = bestTarget;
        
        if (currentTarget != null && Time.frameCount % 60 == 0)
        {
            // Debug.Log($"[{unitName}] ✓ Alvo encontrado: {currentTarget.name} (prioridade: {targetPriority})");
        }
        else if (currentTarget == null && Time.frameCount % 120 == 0)
        {
            // Debug.Log($"[{unitName}] ✗ Nenhum alvo válido encontrado");
        }
    }

    /// <summary>
    /// Calcula prioridade do alvo baseado na estratégia configurada.
    /// Retorna valor menor = maior prioridade
    /// </summary>
    protected virtual float CalculateTargetPriority(UnitBase target)
    {
        switch (targetPriority)
        {
            case TargetPriority.Closest:
                // Menor distância = maior prioridade
                return Vector2.Distance(transform.position, target.transform.position);

            case TargetPriority.LowestHealth:
                // Menor HP = maior prioridade (foca em eliminar alvos fracos)
                return target.currentHealth;

            case TargetPriority.HighestDamage:
                // Maior dano = maior prioridade (neutraliza ameaças primeiro)
                // Inverte o valor para que maior dano tenha menor número
                return -target.damage;

            default:
                return Vector2.Distance(transform.position, target.transform.position);
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
        // Debug.Log($"{unitName} recebeu {damageAmount} de dano. HP: {currentHealth}/{maxHealth}");

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
        
        // Debug.Log($"{unitName} morreu!");
        
        // Dispara evento ANTES de iniciar destruição
        OnDeath?.Invoke(this);

        // Desabilita colliders para parar de receber ataques
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

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
