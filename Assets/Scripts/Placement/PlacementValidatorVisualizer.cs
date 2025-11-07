using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlacementValidatorVisualizer : MonoBehaviour
{
    public Sprite rangeSprite; // small circle sprite (assign)
    private GameObject overlay;

    void Start()
    {
        if (rangeSprite == null) return;
        overlay = new GameObject("range_overlay");
        var sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = rangeSprite;
        sr.sortingLayerName = "UI";
        sr.color = new Color(1f,1f,1f,0.12f);
        overlay.transform.SetParent(transform, false);
        overlay.transform.localPosition = Vector3.zero;
        overlay.transform.localScale = Vector3.one * 2.5f;
    }
}
