using UnityEngine;
using System;

/// <summary>
/// Representa o core/base do jogador que os inimigos tentam destruir.
/// Quando destruído, o jogador perde.
/// </summary>
public class PlayerCore : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private float maxHealth = 500f;
    [SerializeField] private float currentHealth;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.blue;
    [SerializeField] private Color damagedColor = Color.red;

    // Eventos
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action OnDestroyed;

    private bool isDestroyed = false;

    void Awake()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Garante que tem a tag correta
        if (!gameObject.CompareTag("PlayerCore"))
        {
            gameObject.tag = "PlayerCore";
            Debug.Log("[PlayerCore] Tag 'PlayerCore' adicionada automaticamente");
        }
    }

    void Start()
    {
        // Configura layer se necessário
        if (gameObject.layer == 0) // Default
        {
            int structuresLayer = LayerMask.NameToLayer("Structures");
            if (structuresLayer != -1)
            {
                gameObject.layer = structuresLayer;
            }
        }

        Debug.Log($"[PlayerCore] Inicializado com {currentHealth}/{maxHealth} HP");
    }

    public void TakeDamage(float damage)
    {
        if (isDestroyed) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"[PlayerCore] Dano recebido: {damage}. HP restante: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Feedback visual
        UpdateVisuals();

        // Verifica destruição
        if (currentHealth <= 0)
        {
            DestroyCore();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        // Muda cor baseado no HP
        float healthPercent = currentHealth / maxHealth;
        spriteRenderer.color = Color.Lerp(damagedColor, normalColor, healthPercent);
    }

    private void DestroyCore()
    {
        if (isDestroyed) return;

        isDestroyed = true;
        Debug.Log("[PlayerCore] CORE DESTRUÍDO!");

        OnDestroyed?.Invoke();

        // Efeito visual de destruição
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.black;
        }

        // Aguarda um pouco antes de destruir
        Destroy(gameObject, 1f);
    }

    // Métodos públicos
    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDestroyed() => isDestroyed;

    // Para debug
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}
