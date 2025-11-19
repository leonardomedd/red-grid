# Configuração do Prefab Enemy Tank

## 📋 Passos para Criar enemy_tank_placeholder.prefab no Unity

### 1. Duplicar Prefab Existente
```
1. No Project: Assets/Prefabs/enemy_basic_placeholder.prefab
2. Ctrl+D (duplicar)
3. Renomear para: enemy_tank_placeholder.prefab
```

### 2. Configurar GameObject
```
Name: enemy_tank_placeholder
Tag: Enemies
Layer: Enemies (7)
```

### 3. Components Necessários

#### Transform
```
Position: (0, 0, 0)
Rotation: (0, 0, 0)
Scale: (1.5, 1.5, 1) // 50% maior que BasicEnemy
```

#### SpriteRenderer
```
Sprite: Square ou Circle (placeholder)
Color: RGB(50, 50, 100) - Azul escuro
Sorting Layer: Units
Order in Layer: 0
```

#### Rigidbody2D ✅ CRÍTICO
```
Body Type: Kinematic
Simulated: ✅
Use Auto Mass: ❌
Mass: 1
Linear Drag: 0
Angular Drag: 0.05
Gravity Scale: 0
Interpolate: None
Sleeping Mode: Start Awake
Collision Detection: Discrete
Constraints:
  - Freeze Position: ❌ X, ❌ Y
  - Freeze Rotation: ✅ Z
```

#### CircleCollider2D
```
Is Trigger: ❌
Radius: 0.45 (maior que BasicEnemy's 0.3)
Offset: (0, 0)
```

#### Script: EnemyTank
```
--- Unit Stats ---
Unit Name: "Tanque Opressor"
Max Health: 150
Damage: 25
Attack Range: 2.5
Attack Cooldown: 2.0
Move Speed: 1.2

--- Combat ---
Enemy Layer: Units, Structures
Is Enemy: ✅
Target Priority: Closest (ou configurar HighestDamage)

--- Visual Feedback ---
Normal Color: RGB(70, 70, 120) - Azul metálico
Hurt Color: RGB(255, 0, 0) - Vermelho
Hurt Flash Duration: 0.1

--- Tank Special ---
Armor Reduction: 0.3 (30%)
```

#### HealthBarCanvas (Prefab Child)
```
Já vem do prefab original - manter como está
```

---

## 🎨 Sprites Placeholder Sugeridos

### BasicEnemy
```
Forma: Circle pequeno
Cor: RGB(180, 50, 50) - Vermelho claro
Tamanho: 1x1
Visual: Leve, ágil
```

### EnemyTank
```
Forma: Square ou retângulo
Cor: RGB(50, 50, 100) - Azul escuro
Tamanho: 1.5x1.5
Visual: Pesado, robusto
Adicionar: Pequeno "X" ou cruz no sprite para parecer blindagem
```

---

## 🔧 Configuração Rápida via Inspector

### Depois de criar o prefab:

1. **Selecionar prefab** no Project
2. **Inspector → EnemyTank component**
3. **Arrastar referências:**
   - Sprite Renderer → auto-preenchido
   - Rigidbody2D → auto-preenchido (se não, arrastar do componente)

4. **Testar no WaveManager:**
   ```
   Wave 4 (Tank Test):
   - Wave Name: "Onda de Tanques"
   - Enemies:
     - Enemy Prefab: enemy_tank_placeholder
     - Count: 2
   ```

---

## ⚠️ Checklist de Validação

- [ ] Prefab criado em Assets/Prefabs/
- [ ] Tag "Enemies" configurada
- [ ] Layer "Enemies" (7) configurado
- [ ] Rigidbody2D em Kinematic mode
- [ ] Gravity Scale = 0
- [ ] Freeze Rotation Z ativado
- [ ] EnemyTank script attached
- [ ] CircleCollider2D maior que BasicEnemy
- [ ] Sprite diferente (azul escuro)
- [ ] Testado em uma wave

---

## 🧪 Teste de Funcionalidade

### Scene Test:
1. Adicionar ao WaveManager uma wave só com tanks
2. Play mode
3. Verificar:
   - ✅ Spawna corretamente
   - ✅ Move em direção ao PlayerCore (DEVAGAR)
   - ✅ Ataca aliados próximos
   - ✅ Recebe dano reduzido (armadura 30%)
   - ✅ Leva mais hits para morrer (150 HP vs 40 HP)
   - ✅ Causa mais dano (25 vs 10)

### Balance Test:
```
1 Tank = aproximadamente 4 BasicEnemies em força
HP: 150 vs 40 (3.75x)
Dano: 25 vs 10 (2.5x)
Velocidade: 1.2 vs 2.2 (0.54x - muito mais lento)
```

---

## 📊 Comparação de Stats

| Stat | BasicEnemy | EnemyTank | Ratio |
|------|------------|-----------|-------|
| **HP** | 40 | 150 | 3.75x |
| **Dano** | 10 | 25 | 2.5x |
| **Velocidade** | 2.2 | 1.2 | 0.54x |
| **Attack Range** | 2.0 | 2.5 | 1.25x |
| **Cooldown** | 1.3s | 2.0s | 1.54x |
| **Armadura** | 0% | 30% | - |
| **Tamanho** | 1.0 | 1.5 | 1.5x |

---

**Status:** Aguardando criação manual do prefab no Unity Editor
