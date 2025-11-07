using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente simples para controlar barra de progresso de construção.
/// Deve estar em um Canvas world-space com Image (fillAmount).
/// </summary>
public class BuildProgressBar : MonoBehaviour
{
    public Image fillImage; // arraste o componente Image aqui no inspector

    public void SetProgress(float progress)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(progress);
        }
    }
}
