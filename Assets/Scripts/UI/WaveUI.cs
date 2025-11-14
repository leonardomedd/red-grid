using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI que mostra informações sobre as waves:
/// - Wave atual
/// - Inimigos restantes
/// - Tempo até próxima wave
/// - Status de vitória/derrota
/// </summary>
public class WaveUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI enemiesAliveText;
    [SerializeField] private TextMeshProUGUI nextWaveTimerText;
    [SerializeField] private GameObject nextWavePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("Wave Start Animation")]
    [SerializeField] private GameObject waveStartPanel;
    [SerializeField] private TextMeshProUGUI waveStartText;
    [SerializeField] private float waveStartDisplayTime = 2f;

    private bool gameEnded = false;

    void Start()
    {
        // Encontra WaveManager se não configurado
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        if (waveManager == null)
        {
            Debug.LogError("[WaveUI] WaveManager não encontrado!");
            return;
        }

        // Subscreve aos eventos
        waveManager.OnWaveStart += OnWaveStart;
        waveManager.OnWaveComplete += OnWaveComplete;
        waveManager.OnAllWavesComplete += OnVictory;
        waveManager.OnGameOver += OnDefeat;

        // Inicializa UI
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (waveStartPanel != null) waveStartPanel.SetActive(false);
    }

    void Update()
    {
        if (waveManager == null || gameEnded) return;

        UpdateWaveInfo();
        UpdateEnemyCount();
        UpdateNextWaveTimer();
    }

    private void UpdateWaveInfo()
    {
        if (waveNumberText != null)
        {
            int current = waveManager.GetCurrentWave();
            int total = waveManager.GetTotalWaves();
            waveNumberText.text = $"Wave: {current}/{total}";
        }
    }

    private void UpdateEnemyCount()
    {
        if (enemiesAliveText != null)
        {
            int enemies = waveManager.GetEnemiesAlive();
            enemiesAliveText.text = $"Inimigos: {enemies}";
        }
    }

    private void UpdateNextWaveTimer()
    {
        float timeUntilNext = waveManager.GetTimeUntilNextWave();

        if (timeUntilNext > 0)
        {
            // Mostra timer
            if (nextWavePanel != null) nextWavePanel.SetActive(true);
            if (nextWaveTimerText != null)
            {
                nextWaveTimerText.text = $"Próxima Wave em: {timeUntilNext:F1}s";
            }
        }
        else
        {
            // Esconde timer durante wave
            if (nextWavePanel != null) nextWavePanel.SetActive(false);
        }
    }

    private void OnWaveStart(int waveNumber)
    {
        Debug.Log($"[WaveUI] Wave {waveNumber} iniciada");
        
        // Animação de início de wave
        if (waveStartPanel != null && waveStartText != null)
        {
            waveStartText.text = $"WAVE {waveNumber}";
            StartCoroutine(ShowWaveStartAnimation());
        }
    }

    private System.Collections.IEnumerator ShowWaveStartAnimation()
    {
        waveStartPanel.SetActive(true);
        yield return new WaitForSeconds(waveStartDisplayTime);
        waveStartPanel.SetActive(false);
    }

    private void OnWaveComplete(int waveNumber)
    {
        Debug.Log($"[WaveUI] Wave {waveNumber} completada");
    }

    private void OnVictory()
    {
        Debug.Log("[WaveUI] VITÓRIA!");
        gameEnded = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (nextWavePanel != null)
        {
            nextWavePanel.SetActive(false);
        }
    }

    private void OnDefeat()
    {
        Debug.Log("[WaveUI] DERROTA!");
        gameEnded = true;

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (nextWavePanel != null)
        {
            nextWavePanel.SetActive(false);
        }
    }

    void OnDestroy()
    {
        // Limpa eventos
        if (waveManager != null)
        {
            waveManager.OnWaveStart -= OnWaveStart;
            waveManager.OnWaveComplete -= OnWaveComplete;
            waveManager.OnAllWavesComplete -= OnVictory;
            waveManager.OnGameOver -= OnDefeat;
        }
    }
}
