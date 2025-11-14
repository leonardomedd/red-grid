# Sistema de Ondas (Wave System) - Guia de Setup

## ✅ Scripts Criados

1. **WaveManager.cs** - Gerenciador principal do sistema de ondas
2. **WaveUI.cs** - Interface de usuário para waves
3. **PlayerCore.cs** - Core/base do jogador que os inimigos atacam
4. **WaveSystemTester.cs** - Ferramenta de debug para configurar waves rapidamente

## 🎮 Como Configurar (Opção 1 - Automática)

### 1. Setup Rápido com WaveSystemTester

```
1. Crie um GameObject vazio chamado "WaveSystem"
2. Adicione os componentes:
   - WaveManager
   - WaveSystemTester
3. No WaveSystemTester Inspector:
   - Arraste o prefab "enemy_basic_placeholder" para basicEnemyPrefab
   - Marque "Auto Setup Waves"
4. Execute o jogo - spawn points e core serão criados automaticamente
```

### 2. Configurar Waves no Inspector

No `WaveManager`:

**Wave 1 (Fácil):**
- Wave Name: "Primeira Onda"
- Enemies:
  - Enemy Prefab: enemy_basic_placeholder
  - Count: 3

**Wave 2 (Média):**
- Wave Name: "Segunda Onda"
- Enemies:
  - Enemy Prefab: enemy_basic_placeholder
  - Count: 5

**Wave 3 (Difícil):**
- Wave Name: "Terceira Onda"
- Enemies:
  - Enemy Prefab: enemy_basic_placeholder
  - Count: 8

**Configurações:**
- Time Between Waves: 10s
- Time Between Spawns: 0.5s
- Auto Start Waves: ✅

## 🎮 Como Configurar (Opção 2 - Manual)

### 1. Criar WaveManager

```
1. Crie GameObject vazio: "WaveManager"
2. Adicione componente: WaveManager
```

### 2. Criar Spawn Points

```
1. Crie GameObject vazio: "SpawnPoints"
2. Crie 4 filhos (posições sugeridas):
   - SpawnPoint_TopRight: (10, 10, 0)
   - SpawnPoint_TopLeft: (-10, 10, 0)
   - SpawnPoint_BottomRight: (10, -10, 0)
   - SpawnPoint_BottomLeft: (-10, -10, 0)
3. Arraste os 4 spawns para o array Spawn Points do WaveManager
```

### 3. Criar Player Core

```
1. Crie GameObject: Cube
2. Renomeie para "PlayerCore"
3. Configure:
   - Tag: PlayerCore (crie se não existir)
   - Layer: Structures
   - Position: (0, 0, 0)
   - Scale: (2, 2, 2)
4. Adicione componentes:
   - BoxCollider2D (size: 2x2)
   - PlayerCore script
5. Arraste para "Player Core" no WaveManager
```

### 4. Criar Wave UI

```
1. Crie Canvas (se não existir)
2. Crie UI Layout:
   Canvas
   └── WaveInfoPanel
       ├── WaveNumberText (TextMeshPro)
       ├── EnemiesAliveText (TextMeshPro)
       └── NextWavePanel
           └── NextWaveTimerText (TextMeshPro)
   └── WaveStartPanel (inicialmente desativado)
       └── WaveStartText (TextMeshPro)
   └── VictoryPanel (inicialmente desativado)
       └── VictoryText
   └── DefeatPanel (inicialmente desativado)
       └── DefeatText

3. Adicione WaveUI script ao Canvas
4. Configure referências no Inspector
```

## 📋 Configuração de Tags e Layers

### Tags Necessárias:
- **PlayerCore** - Para o core do jogador
- **Enemies** - Para unidades inimigas (já existe)
- **Units** - Para unidades aliadas (já existe)

### Layers Necessárias:
- **Structures (9)** - Para o Player Core e estruturas
- **Enemies (7)** - Para inimigos (já existe)
- **Units (8)** - Para aliados (já existe)

## 🎯 Fluxo do Sistema

```
1. PLACEMENT PHASE (manual ou automático)
   └── Posicione unidades aliadas

2. WAVE SYSTEM START
   └── WaveManager.StartWaveSystem()

3. WAITING STATE
   └── Timer conta até iniciar próxima wave
   └── UI mostra countdown

4. SPAWNING STATE
   └── Spawna inimigos em spawn points
   └── Define PlayerCore como objetivo
   └── Delay entre spawns

5. FIGHTING STATE
   └── Inimigos se movem em direção ao core
   └── Aliados detectam e atacam inimigos
   └── UI mostra inimigos restantes

6. WAVE COMPLETE
   └── Todos os inimigos derrotados
   └── Volta para WAITING STATE
   └── Próxima wave ou vitória

7. VICTORY
   └── Todas as waves derrotadas
   └── OnAllWavesComplete evento
   └── Victory UI aparece

8. DEFEAT
   └── PlayerCore destruído
   └── OnGameOver evento
   └── Defeat UI aparece
```

## 🔧 Eventos Disponíveis

```csharp
// No WaveManager
waveManager.OnWaveStart += (waveNumber) => { };
waveManager.OnWaveComplete += (waveNumber) => { };
waveManager.OnAllWavesComplete += () => { };
waveManager.OnGameOver += () => { };

// No PlayerCore
playerCore.OnHealthChanged += (current, max) => { };
playerCore.OnDestroyed += () => { };
```

## 🐛 Debug e Testes

### Métodos Úteis:
```csharp
waveManager.ForceStartNextWave();  // Pula timer
waveManager.GetCurrentWave();      // Wave atual
waveManager.GetTotalWaves();       // Total de waves
waveManager.GetEnemiesAlive();     // Inimigos vivos
waveManager.IsGameEnded();         // Jogo terminou?
```

### Visualização:
- Spawn points aparecem como esferas vermelhas no Scene view
- Linhas amarelas conectam spawns ao core (Gizmos)

## ⚙️ Parâmetros Ajustáveis

### WaveManager:
- `timeBetweenWaves` - Tempo de espera entre waves (padrão: 10s)
- `timeBetweenSpawns` - Delay entre spawn de cada inimigo (padrão: 0.5s)
- `useRandomSpawnPoints` - Aleatório ou sequencial (padrão: true)
- `autoStartWaves` - Inicia automaticamente (padrão: true)

### PlayerCore:
- `maxHealth` - HP total do core (padrão: 500)

### Wave (estrutura de dados):
```csharp
[Serializable]
public struct Wave {
    public string waveName;
    public List<EnemySpawnData> enemies;
}

[Serializable]
public struct EnemySpawnData {
    public GameObject enemyPrefab;
    public int count;
}
```

## 🎨 Exemplo de Waves Balanceadas

```
Wave 1 - Tutorial:
- 3 inimigos básicos
- Tempo: 15s entre waves

Wave 2 - Aquecimento:
- 5 inimigos básicos
- Tempo: 12s entre waves

Wave 3 - Desafio:
- 8 inimigos básicos
- Tempo: 10s entre waves

Wave 4 - Boss (futuro):
- 1 boss + 4 adds
- Tempo: 20s entre waves
```

## ✅ Checklist de Implementação

- [x] WaveManager.cs criado
- [x] Estrutura de dados Wave e EnemySpawnData
- [x] Sistema de spawn em posições definidas
- [x] Timer entre waves
- [x] WaveUI.cs criado
- [x] PlayerCore.cs criado
- [x] Sistema de vitória (OnAllWavesComplete)
- [x] Sistema de derrota (OnGameOver)
- [ ] Configurar 3 waves de teste no Unity Inspector
- [ ] Criar UI visual no Canvas
- [ ] Testar ciclo completo placement → waves → vitória/derrota

## 🚀 Próximos Passos

1. **Arte e Polish:**
   - Sprites para spawn points
   - Efeitos visuais de spawn
   - Animação de morte do core
   - Partículas de vitória/derrota

2. **Balanceamento:**
   - Testar dificuldade das waves
   - Ajustar HP do core
   - Timing entre waves

3. **Features Adicionais:**
   - Tipos diferentes de inimigos
   - Boss waves
   - Recompensas por wave
   - Sistema de pontuação

---

**Status:** Sistema core completo ✅  
**Testado:** Aguardando configuração no Unity Editor  
**Documentado:** ✅
