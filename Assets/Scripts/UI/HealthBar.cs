using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de HP que segue a unidade no mundo.
/// Mostra visualmente a saúde restante.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Canvas canvas;
    [SerializeField] private UnitBase unit;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.8f, 0);
    [SerializeField] private bool hideWhenFull = true;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Se não tem referência, busca no pai
        if (unit == null)
        {
            unit = GetComponentInParent<UnitBase>();
        }

        if (unit == null)
        {
            Debug.LogError($"[HealthBar] Não encontrou UnitBase no pai de {gameObject.name}!");
            return;
        }

        // Configura canvas
        if (canvas != null)
        {
            canvas.worldCamera = mainCamera;
        }
        else
        {
            Debug.LogWarning($"[HealthBar] Canvas não está assignado em {gameObject.name}");
        }

        // Inscreve no evento de dano
        unit.OnDamageTaken += OnUnitDamaged;
        unit.OnDeath += OnUnitDeath;

        // Esconde inicialmente se vida está cheia
        UpdateHealthBar();
        
        // Debug.Log($"[HealthBar] Inicializado para {unit.GetUnitName()}");
    }

    private void LateUpdate()
    {
        // Posiciona acima da unidade
        if (unit != null)
        {
            transform.position = unit.transform.position + offset;
        }

        // Sempre olha para a câmera (billboard)
        if (mainCamera != null && canvas != null)
        {
            canvas.transform.rotation = mainCamera.transform.rotation;
        }
    }

    private void OnUnitDamaged(UnitBase damagedUnit)
    {
        UpdateHealthBar();
    }

    private void OnUnitDeath(UnitBase deadUnit)
    {
        // Esconde barra quando morre
        gameObject.SetActive(false);
    }

    private void UpdateHealthBar()
    {
        if (unit == null || fillImage == null) return;

        float healthPercent = unit.GetHealthPercent();
        fillImage.fillAmount = healthPercent;

        // Muda cor baseado na saúde
        if (healthPercent > 0.6f)
            fillImage.color = Color.green;
        else if (healthPercent > 0.3f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;

        // Esconde se vida cheia
        if (hideWhenFull && healthPercent >= 0.99f)
        {
            if (canvas != null) canvas.gameObject.SetActive(false);
        }
        else
        {
            if (canvas != null) canvas.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        // Remove inscrições
        if (unit != null)
        {
            unit.OnDamageTaken -= OnUnitDamaged;
            unit.OnDeath -= OnUnitDeath;
        }
    }
}
