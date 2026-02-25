using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GhostFollower : MonoBehaviour
{
    private UnitCardUI cardOwner;
    private GameObject realPrefab;
    private int cost;
    private bool isStructure;
    private int unitsToSpawn = 1; // Quantidade de unidades a spawnar

    private SpriteRenderer sr;
    private Color normalColor;
    private Color blockedColor;
    private bool canPlace = true;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        normalColor = new Color(1f, 1f, 1f, 0.5f);
        blockedColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    }

    public void Init(UnitCardUI owner, GameObject prefab, int placeCost, bool isStruct, int unitCount = 1)
    {
        cardOwner = owner;
        realPrefab = prefab;
        cost = placeCost;
        isStructure = isStruct;
        unitsToSpawn = unitCount;
    }

    void Update()
    {
        // --- Leitura da posição do mouse e cliques (suporta Input System e Input Legacy)
        Vector2 screenPos;
        bool leftClicked = false;
        bool rightClicked = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            screenPos = Mouse.current.position.ReadValue();
            leftClicked = Mouse.current.leftButton.wasPressedThisFrame;
            rightClicked = Mouse.current.rightButton.wasPressedThisFrame;
        }
        else
#endif
        {
            screenPos = Input.mousePosition;
            leftClicked = Input.GetMouseButtonDown(0);
            rightClicked = Input.GetMouseButtonDown(1);
        }

        // --- Converte screen → world (método otimizado para câmeras ortográficas)
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos;
        
        if (cam.orthographic)
        {
            // Para câmeras ortográficas, projeta direto no plano Z=0
            // Usa a posição da câmera como referência de profundidade
            Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane);
            worldPos = cam.ScreenToWorldPoint(screenPoint);
            
            // Projeta no plano Z=0 considerando a direção da câmera
            if (Mathf.Abs(cam.transform.forward.z) > 0.01f)
            {
                float t = -worldPos.z / cam.transform.forward.z;
                worldPos += cam.transform.forward * t;
            }
            worldPos.z = 0f;
        }
        else
        {
            // Fallback para câmeras em perspectiva
            Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);
            Ray ray = cam.ScreenPointToRay(screenPos);
            if (gamePlane.Raycast(ray, out float distance))
            {
                worldPos = ray.GetPoint(distance);
                worldPos.z = 0f;
            }
            else
            {
                worldPos = transform.position; // mantém posição atual se falhar
            }
        }
        
        transform.position = worldPos;

        // --- Atualiza cor de transparência com base na validade
        sr.color = canPlace ? normalColor : blockedColor;

        // --- Clique esquerdo: tenta construir
        if (leftClicked)
        {
            if (canPlace)
            {
                PlacerManager.Instance.RequestPlaceMultiple(realPrefab, worldPos, cost, isStructure, unitsToSpawn);
                if (cardOwner != null) cardOwner.CancelPlacement();
            }
        }

        // --- Clique direito ou ESC: cancela construção
        bool escPressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            escPressed = Keyboard.current.escapeKey.wasPressedThisFrame;
        }
#else
        escPressed = Input.GetKeyDown(KeyCode.Escape);
#endif

        if (rightClicked || escPressed)
        {
            if (cardOwner != null) cardOwner.CancelPlacement();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // se o ghost encostar em algo, bloqueia posicionamento
        if (collision.CompareTag("Structure") || collision.CompareTag("Unit"))
        {
            canPlace = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // libera novamente quando sai da área
        if (collision.CompareTag("Structure") || collision.CompareTag("Unit"))
        {
            canPlace = true;
        }
    }
}
