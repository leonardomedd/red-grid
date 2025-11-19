# Sistema de Combate - Red Grid: Rise of the Comrades

## Visão Geral
Sistema de combate automático baseado em IA onde unidades detectam inimigos, se movem em direção a eles e atacam automaticamente. Implementado usando Unity 6 (2025) LTS com Physics2D e sistema de layers.

## Arquitetura

### Hierarquia de Classes

```
UnitBase (Abstract)
├── ComradeRecruit (Ally)
├── WorkerBrigade (Ally)
└── BasicEnemy (Enemy)
```

### UnitBase.cs
**Localização:** `Assets/Scripts/Units/UnitBase.cs`

Classe abstrata base para todas as unidades do jogo. Implementa:

#### Propriedades Principais
```csharp
// Stats
public float maxHealth = 100f;
public float currentHealth;
public float attackDamage = 10f;
public float attackRange = 2.5f;
public float attackSpeed = 1f;
public float moveSpeed = 2f;

// Estado
public bool isEnemy = false;
public bool isDead = false;
protected LayerMask enemyLayer;
```

#### State Machine
- **Idle**: Procura inimigos próximos
- **Moving**: Move-se em direção ao alvo
- **Attacking**: Ataca quando em range
- **Dead**: Unidade morreu

#### Métodos Principais

##### FindTarget()
Detecta inimigos usando `Physics2D.OverlapCircleAll` com LayerMask:
```csharp
Collider2D[] hits = Physics2D.OverlapCircleAll(
    transform.position, 
    attackRange * 1.5f, 
    enemyLayer
);
```

##### MoveTowardsTarget()
Move a unidade em direção ao alvo:
```csharp
transform.position = Vector2.MoveTowards(
    transform.position, 
    currentTarget.position, 
    moveSpeed * Time.deltaTime
);
```

##### Attack()
Sistema de ataque baseado em cooldown:
```csharp
private float nextAttackTime = 0f;

void Attack() {
    if (Time.time >= nextAttackTime) {
        // Aplica dano
        targetUnit.TakeDamage(attackDamage);
        nextAttackTime = Time.time + (1f / attackSpeed);
    }
}
```

##### TakeDamage()
Recebe dano e dispara eventos:
```csharp
public virtual void TakeDamage(float damage) {
    currentHealth -= damage;
    OnHealthChanged?.Invoke(currentHealth, maxHealth);
    
    if (currentHealth <= 0) {
        Die();
    }
}
```

#### Eventos
```csharp
public event Action<float, float> OnHealthChanged;  // (current, max)
public event Action OnDeath;
```

---

## Unidades Implementadas

### 1. ComradeRecruit (Recruta Camarada)
**Arquivo:** `Assets/Scripts/Units/ComradeRecruit.cs`

Unidade de infantaria básica balanceada.

**Stats:**
- HP: 50
- Dano: 8
- Range: 2.5
- Velocidade: 2.0
- Attack Speed: 1.0

**Configuração:**
```csharp
protected override void Awake() {
    base.Awake();
    isEnemy = false;  // Define ANTES de base.Start()
}

protected override void Start() {
    base.Start();
    enemyLayer = LayerMask.GetMask("Enemies");
}
```

---

### 2. WorkerBrigade (Brigada Operária)
**Arquivo:** `Assets/Scripts/Units/WorkerBrigade.cs`

Unidade tanque com stance defensiva.

**Stats:**
- HP: 80
- Dano: 15
- Range: 1.5 (melee)
- Velocidade: 1.5
- Attack Speed: 0.8

**Mecânica Especial - Defensive Stance:**
```csharp
private bool inDefensiveStance = true;

public override void TakeDamage(float damage) {
    if (inDefensiveStance) {
        damage *= 0.8f;  // 20% de redução
    }
    base.TakeDamage(damage);
}

protected override void IdleBehavior() {
    inDefensiveStance = true;
    base.IdleBehavior();
}

protected override void MovingBehavior() {
    inDefensiveStance = false;
    base.MovingBehavior();
}
```

---

### 3. BasicEnemy (Reacionário)
**Arquivo:** `Assets/Scripts/Units/BasicEnemy.cs`

Inimigo básico que ataca unidades e estruturas.

**Stats:**
- HP: 40
- Dano: 10
- Range: 2.0
- Velocidade: 2.2
- Attack Speed: 1.3

**IA - Busca de Objetivos:**
```csharp
public GameObject targetObjective;  // Estrutura alvo

protected override void Start() {
    base.Start();
    enemyLayer = LayerMask.GetMask("Units", "Structures");
    
    if (targetObjective == null) {
        FindObjective();
    }
}

private void FindObjective() {
    GameObject[] structures = GameObject.FindGameObjectsWithTag("Structure");
    if (structures.Length > 0) {
        targetObjective = structures[0];
    }
}
```

**Comportamento Idle:**
- Se tiver unidade inimiga em range → ataca
- Senão, move-se em direção ao objetivo (estrutura)

---

### 4. EnemyTank (Tanque Opressor) ✨ NOVO
**Arquivo:** `Assets/Scripts/Units/EnemyTank.cs`

Inimigo pesado blindado com alto HP e dano devastador.

**Stats:**
- HP: 150 (3.75x BasicEnemy)
- Dano: 25 (2.5x BasicEnemy)
- Range: 2.5
- Velocidade: 1.2 (54% do BasicEnemy - MUITO LENTO)
- Attack Speed: 2.0 (mais lento)
- **Armadura: 30% de redução de dano** 🛡️

**Mecânica Especial - Armadura:**
```csharp
[SerializeField] private float armorReduction = 0.3f; // 30%

public override void TakeDamage(float damageAmount, UnitBase attacker) {
    float reducedDamage = damageAmount * (1f - armorReduction);
    base.TakeDamage(reducedDamage, attacker);
}
```

**Papel Tático:**
- Tanque de linha de frente
- Absorve muito dano
- Ameaça letal se chegar ao PlayerCore
- Lento = vulnerável a kiting
- Requer foco de fogo múltiplo para abater

**Balanceamento:**
```
Força Equivalente: 1 Tank ≈ 4 BasicEnemies
- 3.75x mais HP
- 2.5x mais dano
- 30% de redução de dano
- MAS: 46% mais lento (janela maior para counter)
```

---
- Senão, move-se em direção ao objetivo (estrutura)

---

## Sistema de Layers

### Configuração de Layers
- **Layer 7:** `Enemies` - Unidades inimigas
- **Layer 8:** `Units` - Unidades aliadas
- **Layer 9:** `Structures` - Estruturas (base, torres, etc)

### Layer Collision Matrix
**Configurado em:** `ProjectSettings/Physics2DSettings.asset`

Todas as interações habilitadas:
- Units ↔ Enemies ✅
- Units ↔ Structures ✅
- Enemies ↔ Structures ✅

### LayerMask - Bug Fix Crítico

❌ **Método ERRADO (não funciona):**
```csharp
int enemyLayerNumber = LayerMask.NameToLayer("Enemies");
enemyLayer = 1 << enemyLayerNumber;  // BUG: cria int, não LayerMask
```

✅ **Método CORRETO:**
```csharp
enemyLayer = LayerMask.GetMask("Enemies");
// Ou múltiplas layers:
enemyLayer = LayerMask.GetMask("Units", "Structures");
```

**Motivo:** `LayerMask` é um struct, não um int. `LayerMask.GetMask()` cria a estrutura corretamente, enquanto bit-shifting manual cria apenas um int que não é reconhecido por `Physics2D.OverlapCircleAll()`.

---

## Sistema de Priorização de Alvos 🎯

### Target Priority System
**Implementado em:** `UnitBase.cs`

Sistema configurável que permite cada unidade escolher qual inimigo atacar primeiro.

#### Tipos de Priorização

**1. Closest (Mais Próximo)** - Padrão
```csharp
targetPriority = TargetPriority.Closest;
```
- Ataca o inimigo mais próximo
- Melhor para: Unidades defensivas, proteção de área
- Minimiza movimento, maximiza DPS

**2. LowestHealth (Mais Fraco)**
```csharp
targetPriority = TargetPriority.LowestHealth;
```
- Foca em eliminar alvos com menor HP
- Melhor para: Finalizadores, limpeza rápida
- Estratégia: Reduzir número de inimigos rapidamente

**3. HighestDamage (Maior Ameaça)**
```csharp
targetPriority = TargetPriority.HighestDamage;
```
- Prioriza inimigos que causam mais dano
- Melhor para: Proteção do core, controle de ameaças
- Estratégia: Neutralizar perigos antes que causem estrago

#### Implementação

```csharp
public enum TargetPriority {
    Closest,        // Alvo mais próximo (padrão)
    LowestHealth,   // Alvo mais fraco (menor HP)
    HighestDamage   // Maior ameaça (maior dano)
}

[SerializeField] protected TargetPriority targetPriority = TargetPriority.Closest;

protected virtual float CalculateTargetPriority(UnitBase target) {
    switch (targetPriority) {
        case TargetPriority.Closest:
            return Vector2.Distance(transform.position, target.transform.position);
        
        case TargetPriority.LowestHealth:
            return target.currentHealth;
        
        case TargetPriority.HighestDamage:
            return -target.damage; // Negativo para inverter (maior = menor valor)
        
        default:
            return Vector2.Distance(transform.position, target.transform.position);
    }
}
```

#### Uso Tático

**Exemplo de Composição:**
```
Linha de Frente (WorkerBrigade):
- Target Priority: HighestDamage
- Papel: Protege o core neutralizando tanks inimigos

Linha de Trás (ComradeRecruit):
- Target Priority: LowestHealth
- Papel: Finaliza inimigos feridos rapidamente

Core Defense (Special Unit):
- Target Priority: Closest
- Papel: Resposta rápida a qualquer invasor
```

**Sinergia:**
- Tanks aliados focam ameaças (EnemyTanks)
- DPS foca alvos fracos (BasicEnemies feridos)
- Elimina inimigos mais rápido = menos dano total recebido

---

## Sistema de UI - Health Bars

### HealthBar.cs
**Localização:** `Assets/Scripts/UI/HealthBar.cs`

Barra de vida em World Space que segue as unidades.

#### Configuração
```csharp
public class HealthBar : MonoBehaviour {
    private UnitBase unit;
    private Image fillImage;
    private CanvasGroup canvasGroup;
    
    void Start() {
        unit = GetComponentInParent<UnitBase>();
        unit.OnHealthChanged += UpdateHealthBar;
        unit.OnDeath += OnUnitDeath;
        
        // Começa invisível se HP cheio
        if (unit.currentHealth >= unit.maxHealth) {
            canvasGroup.alpha = 0;
        }
    }
}
```

#### Update de HP
```csharp
private void UpdateHealthBar(float current, float max) {
    float healthPercent = current / max;
    fillImage.fillAmount = healthPercent;
    
    // Muda cor baseado no HP
    if (healthPercent > 0.6f)
        fillImage.color = Color.green;
    else if (healthPercent > 0.3f)
        fillImage.color = Color.yellow;
    else
        fillImage.color = Color.red;
    
    // Mostra barra quando tomar dano
    canvasGroup.alpha = 1;
}
```

#### Posicionamento
```csharp
void LateUpdate() {
    // Fica acima da unidade
    transform.position = unit.transform.position + Vector3.up * 0.8f;
}
```

### Prefab: HealthBarCanvas
**Localização:** `Assets/Prefabs/HealthBarCanvas.prefab`

**Estrutura:**
```
HealthBarCanvas (Canvas - World Space)
└── HealthBarBackground (Image)
    └── HealthBarFill (Image - Fill)
```

**Configurações:**
- Canvas Render Mode: World Space
- Canvas Scale: 0.01
- HealthBarFill Type: Filled (Horizontal)

---

## Ferramentas de Debug

### CombatTester.cs
**Localização:** `Assets/Scripts/Debug/CombatTester.cs`

Spawna unidades automaticamente para testar combate.

```csharp
[Header("Prefabs")]
public GameObject comradePrefab;
public GameObject brigadePrefab;
public GameObject enemyPrefab;

[Header("Spawn Settings")]
public int alliesCount = 3;
public int enemiesCount = 3;
public Vector2 alliesSpawnArea = new Vector2(-5, 0);
public Vector2 enemiesSpawnArea = new Vector2(5, 0);
public float spawnRadius = 2f;

void Start() {
    SpawnTestUnits();
}
```

**Uso:**
1. Adicione o script a um GameObject vazio
2. Arraste os prefabs nos slots
3. Execute o jogo - unidades spawnam e lutam automaticamente
4. Ou use o botão: `Context Menu > Spawn Test Units`

### UnitDebugger.cs
**Localização:** `Assets/Scripts/Debug/UnitDebugger.cs`

Visualiza informações de debug no Inspector em runtime.

```csharp
[Header("Debug Info")]
[SerializeField] private string currentState;
[SerializeField] private string targetName;
[SerializeField] private float distanceToTarget;
[SerializeField] private float currentHP;

void Update() {
    currentState = unit.currentState.ToString();
    targetName = unit.currentTarget?.name ?? "None";
    distanceToTarget = unit.currentTarget != null 
        ? Vector2.Distance(transform.position, unit.currentTarget.position) 
        : 0f;
    currentHP = unit.currentHealth;
}
```

---

## Fluxo de Combate

### 1. Detecção
```
Update() → IdleBehavior() → FindTarget()
         ↓
Physics2D.OverlapCircleAll(position, range, enemyLayer)
         ↓
Filtra inimigos válidos (isEnemy diferente, !isDead)
         ↓
Seleciona o mais próximo
```

### 2. Movimento
```
MovingBehavior()
    ↓
Calcula distância até alvo
    ↓
< attackRange? → Muda para Attacking
    ↓
≥ attackRange? → MoveTowardsTarget()
```

### 3. Ataque
```
AttackingBehavior()
    ↓
Verifica cooldown (Time.time >= nextAttackTime)
    ↓
Aplica dano: targetUnit.TakeDamage(attackDamage)
    ↓
Atualiza nextAttackTime
    ↓
Alvo morreu? → Volta para Idle
Alvo fugiu? → Volta para Moving
```

### 4. Morte
```
TakeDamage(damage)
    ↓
currentHealth <= 0?
    ↓
Die()
    ↓
- currentState = Dead
- isDead = true
- OnDeath.Invoke()
- Destroy(gameObject, 2f)
```

---

## Prefabs Criados

### Unidades Aliadas

#### unit_militia_placeholder.prefab
**Componentes:**
- SpriteRenderer (sprite: unit_militia_placeholder.png)
- BoxCollider2D (size: 0.8 × 0.8)
- ComradeRecruit script
- Layer: Units (8)
- Tag: Units

#### unit_operario_placeholder.prefab
**Componentes:**
- SpriteRenderer (sprite: unit_operario_placeholder.png)
- BoxCollider2D (size: 1.0 × 1.0)
- WorkerBrigade script
- Layer: Units (8)
- Tag: Units

### Unidades Inimigas

#### enemy_basic_placeholder.prefab
**Componentes:**
- SpriteRenderer (sprite: enemy_basic_placeholder.png)
- CircleCollider2D (radius: 0.4)
- BasicEnemy script
- Layer: Enemies (7)
- Tag: Enemies

---

## Tags Configuradas

**Em:** `ProjectSettings/TagManager.asset`

- **Units** - Unidades aliadas
- **Enemies** - Unidades inimigas
- **Structure** - Estruturas (base, torres)

---

## Problemas Resolvidos

### ✅ LayerMask Detection Bug
**Problema:** Physics2D.OverlapCircleAll retornava 0 hits com LayerMask  
**Solução:** Usar `LayerMask.GetMask()` ao invés de bit-shifting manual

### ✅ Rigidbody2D Movement Bug
**Problema:** Unidades com Rigidbody2D não se moviam  
**Soluções Aplicadas:**
1. Usar `rb.MovePosition()` ao invés de `transform.position`
2. Usar `Time.deltaTime` (não `Time.fixedDeltaTime`) em Update()
3. Configurar Rigidbody2D como **Kinematic** (não Dynamic)
4. Gravity Scale = 0
5. Freeze Rotation Z ativado

### ✅ Enemy Objective Assignment
**Problema:** Inimigos tinham targetObjective mas `hasObjective = false`  
**Solução:** Criar property setter que automaticamente seta a flag quando WaveManager atribui objetivo

### ⏳ Health Bars não aparecem visualmente
**Status:** Pendente investigação

**Sintomas:**
- Script HealthBar.cs executa normalmente
- Eventos OnHealthChanged disparam
- Logs mostram inicialização correta
- Mas a UI não aparece na tela

**Possíveis causas:**
1. Canvas World Space com escala incorreta
2. Sorting Layer/Order incorreto
3. Camera não renderiza UI
4. Prefab não está sendo instanciado corretamente no spawn

**Workaround:** Sistema funciona sem barras de vida por enquanto.

---

## Próximos Passos

### Melhorias Sugeridas
1. ✅ Sistema de combate funcional
2. ✅ Sistema de ondas (WaveManager)
3. ✅ Ciclo completo de gameplay (placement → waves → vitória/derrota)
4. ⏳ Corrigir health bars
5. ⏳ Mais tipos de unidades (Intelligentsia, Sabotador, Comandante)
6. ⏳ Estruturas com habilidades ativas
7. ⏳ Sistema de Moral e Instabilidade
8. ⏳ Cartas de líder e habilidades especiais

### Otimizações Futuras
- Object pooling para unidades
- Spatial partitioning para detecção de inimigos
- Cache de LayerMask e componentes
- Reduzir frequência de FindTarget() com timer

---

## Sistema de Ondas (Wave System)

### Componentes Principais

#### WaveManager.cs
**Localização:** `Assets/Scripts/WaveManager.cs`

Gerencia spawning de ondas de inimigos, progressão e condições de vitória/derrota.

**Estruturas de Dados:**
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

**State Machine:**
- **Waiting**: Aguarda timer para próxima wave
- **Spawning**: Instanciando inimigos
- **Fighting**: Inimigos ativos no campo
- **Complete**: Todas as waves derrotadas

**Eventos:**
```csharp
public event Action<int> OnWaveStart;
public event Action<int> OnWaveComplete;
public event Action OnAllWavesComplete; // Vitória
public event Action OnGameOver;         // Derrota
```

#### PlayerCore.cs
**Localização:** `Assets/Scripts/PlayerCore.cs`

Representa o núcleo/base do jogador que deve ser defendido.

**Stats:**
- HP: 500 (padrão)
- Tag: "PlayerCore"
- Layer: Structures

**Eventos:**
```csharp
public event Action<float, float> OnHealthChanged;
public event Action OnDestroyed;
```

#### WaveUI.cs
**Localização:** `Assets/Scripts/UI/WaveUI.cs`

Interface mostrando informações das waves em tempo real.

**Exibe:**
- Número da wave atual
- Inimigos restantes
- Timer até próxima wave
- Animação de início de wave
- Painéis de vitória/derrota

### Configuração de Waves

**Exemplo de 3 Waves Balanceadas:**

```
Wave 1 - Tutorial (3 inimigos):
- Familiarização com sistema
- Tempo entre waves: 10s

Wave 2 - Escalada (5 inimigos):
- Aumento de dificuldade
- Tempo entre waves: 10s

Wave 3 - Desafio Final (8 inimigos):
- Teste completo de defesa
- Vitória ao derrotar todos
```

### Ciclo de Gameplay Completo

```
INÍCIO
  ↓
1. PLACEMENT PHASE
   - Jogador posiciona unidades (recrutamento: 50)
   - Usa sistema de drag-and-drop
   ↓
2. WAVE SYSTEM START
   - WaveManager.StartWaveSystem()
   - Timer inicia (10 segundos)
   ↓
3. WAVE 1 - Spawning
   - 3 inimigos spawnam em spawn points
   - Inimigos recebem PlayerCore como alvo
   ↓
4. COMBAT
   - Inimigos se movem em direção ao core
   - Aliados detectam e atacam (range 2.5)
   - Sistema de dano/morte ativo
   ↓
5. WAVE COMPLETE
   - Todos os inimigos derrotados
   - Aguarda 10 segundos
   ↓
6. WAVE 2 - Spawning (5 inimigos)
   - Repete ciclo de combate
   ↓
7. WAVE 3 - Spawning (8 inimigos)
   - Última wave
   ↓
8. RESULTADO
   ├─ VITÓRIA: Todas as waves derrotadas
   │  - OnAllWavesComplete evento
   │  - Victory UI exibida
   │
   └─ DERROTA: PlayerCore destruído
      - OnGameOver evento
      - Defeat UI exibida
```

### Recursos de Recrutamento

**PlacerManager:**
- Recrutamento Inicial: 50 pontos
- ComradeRecruit: 3 pontos
- WorkerBrigade: 5 pontos

**Estratégia:**
- 50 pontos permitem ~16 Comrades ou ~10 Brigades
- Balancear quantidade vs. qualidade
- Posicionamento estratégico é crucial

---

## Como Usar

### Criar Nova Unidade Aliada

1. Crie novo script herdando de `UnitBase`:
```csharp
public class MinhaUnidade : UnitBase {
    protected override void Awake() {
        base.Awake();
        isEnemy = false;
    }
    
    protected override void Start() {
        base.Start();
        enemyLayer = LayerMask.GetMask("Enemies");
    }
}
```

2. Configure stats no Inspector ou por código
3. Adicione à Layer "Units" e Tag "Units"
4. Adicione Collider2D

### Criar Nova Unidade Inimiga

1. Mesma estrutura, mas `isEnemy = true`
2. LayerMask pode incluir múltiplos alvos:
```csharp
enemyLayer = LayerMask.GetMask("Units", "Structures");
```
3. Layer "Enemies" e Tag "Enemies"

### Sobrescrever Comportamento

```csharp
protected override void IdleBehavior() {
    // Comportamento customizado
    base.IdleBehavior();  // Chama lógica padrão
}

protected override void Attack() {
    // Ataque especial
    base.Attack();
}
```

---

## Referências

- [Unity Physics2D Documentation](https://docs.unity3d.com/ScriptReference/Physics2D.html)
- [LayerMask Documentation](https://docs.unity3d.com/ScriptReference/LayerMask.html)
- GDD: Red Grid - Rise of the Comrades
- Sprint Planning: Cards 7-8 (Sistema de Unidades)

---

**Última atualização:** 13 de Novembro de 2025  
**Versão do Unity:** 6 (2025) LTS  
**Status:** Sistema core funcional ✅ | Wave System implementado ✅
