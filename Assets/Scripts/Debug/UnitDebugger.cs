using UnityEngine;

/// <summary>
/// Script de debug para verificar configuração de unidades.
/// Anexe a qualquer unidade para ver informações no console.
/// </summary>
public class UnitDebugger : MonoBehaviour
{
    private UnitBase unit;

    void Start()
    {
        unit = GetComponent<UnitBase>();
        if (unit != null)
        {
            LogUnitInfo();
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] Não tem componente UnitBase!");
        }
    }

    void Update()
    {
        // Debug a cada 2 segundos
        if (Time.frameCount % 120 == 0 && unit != null)
        {
            LogUnitStatus();
        }
    }

    void LogUnitInfo()
    {
        Debug.Log($"===== UNIT DEBUG: {gameObject.name} =====");
        Debug.Log($"Tag: {gameObject.tag}");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)}");
        Debug.Log($"Unit Name: {unit.GetUnitName()}");
        Debug.Log($"HP: {unit.GetHealthPercent() * 100}%");
        Debug.Log($"Is Dead: {unit.IsDead()}");
        
        // Verifica componentes necessários
        Debug.Log($"Tem SpriteRenderer: {GetComponent<SpriteRenderer>() != null}");
        Debug.Log($"Tem Collider2D: {GetComponent<Collider2D>() != null}");
        Debug.Log($"Tem Rigidbody2D: {GetComponent<Rigidbody2D>() != null}");
        Debug.Log("=====================================");
    }

    void LogUnitStatus()
    {
        if (unit == null || unit.IsDead()) return;
        
        Debug.Log($"[{unit.GetUnitName()}] HP: {unit.GetHealthPercent() * 100:F0}% | Estado: Ativo");
    }

    private void OnDrawGizmos()
    {
        // Desenha nome da unidade acima dela
        if (unit != null)
        {
            Vector3 labelPos = transform.position + Vector3.up * 1.5f;
            
#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"{unit.GetUnitName()}\nHP: {unit.GetHealthPercent() * 100:F0}%");
#endif
        }
    }
}
