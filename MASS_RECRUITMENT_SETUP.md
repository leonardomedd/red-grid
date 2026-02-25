# Guia de Configuração - Recrutamento em Massa e Produção Automática

## Resumo das Mudanças

Implementamos duas novas mecânicas:
1. **Recrutamento em Massa com Ghost Preview**: O botão de operário spawna 3 unidades ao invés de 1, mantendo o sistema de escolher onde posicionar
2. **Produção Automática da Fábrica**: Fábrica gera 1 operário automaticamente a cada X segundos

---

## 1. RECRUTAMENTO EM MASSA (3 OPERÁRIOS)

### Scripts Modificados
- `Assets/Scripts/UnitCardUI.cs` - Adicionado campo `unitsPerPlacement`
- `Assets/Scripts/GhostFollower.cs` - Suporte para múltiplas unidades
- `Assets/Scripts/Placement/PlacerManager.cs` - Novo método `RequestPlaceMultiple()`

### Como Funciona
- Ao clicar no botão, o ghost aparece normalmente
- Você escolhe onde posicionar (sistema de ghost preview mantido)
- Ao confirmar (clique esquerdo), **3 operários** são spawnados em círculo ao redor do ponto clicado
- Cancelar com clique direito ou ESC funciona igual antes

### Configuração no Unity

#### Passo 1: Configurar o Card de Operário
1. Selecione o GameObject com o script `UnitCardUI` (botão de recrutar operário)
2. No Inspector, localize o componente `UnitCardUI`
3. Configure os campos:

**Card Data:**
- **Ghost Prefab**: Mantenha o ghost atual
- **Unit Prefab**: `unit_operario_placeholder` (ou seu prefab de operário)
- **Cost**: `30` (custo pelos 3 operários, ajuste conforme balanceamento)
- **Is Structure**: ❌ Desmarcado

**Multiple Units (NOVO):**
- **Units Per Placement**: `3` ⬅️ **IMPORTANTE: Defina como 3**

#### Passo 2: Testar
1. Entre no Play Mode
2. Clique no botão de operário
3. Posicione o ghost onde desejar
4. Clique esquerdo para confirmar
5. Observe 3 operários serem criados em círculo ao redor do ponto

---

## 2. PRODUÇÃO AUTOMÁTICA DA FÁBRICA

### Scripts Criados
- `Assets/Scripts/Buildings/FactoryProduction.cs`

### Como Funciona
- Fábrica é posicionada usando o sistema de ghost preview (igual estruturas)
- Operários spawnam **automaticamente** ao redor da posição onde você colocou a fábrica
- Produção ocorre a cada X segundos (configurável)
- Operários aparecem em círculo ao redor da fábrica

### Configuração no Unity

#### Passo 1: Criar Card de Fábrica (UnitCardUI)
1. Crie um botão UI para a fábrica
2. Adicione componente `UnitCardUI`
3. Configure:
   - **Ghost Prefab**: Prefab ghost da fábrica
   - **Unit Prefab**: Prefab real da fábrica (com FactoryProduction)
   - **Cost**: Custo para construir a fábrica (ex: 50)
   - **Is Structure**: ✓ Marcado
   - **Units Per Placement**: 1

#### Passo 2: Configurar Prefab da Fábrica

No prefab da fábrica, adicione o componente `FactoryProduction`:

**Production Settings:**
- **Worker Prefab**: `unit_operario_placeholder`
- **Production Interval**: `10` (segundos entre cada operário)
- **Spawn Radius**: `1.5` (raio ao redor da fábrica onde operários aparecem)
- **Auto Start Production**: ✓ Marcado (inicia automaticamente)

**Cost & Resources:**
- **Requires Resources**: ✓ Marcado (consome recursos do PlacerManager)
- **Production Cost**: `5` (custo por operário)

**Limits (Opcional):**
- **Has Production Limit**: Desmarcado (produção infinita)
- **Max Units Produced**: `-1` (ignora se limit desmarcado)

**Visual Feedback:**
- **Factory Renderer**: Arraste o `SpriteRenderer` da fábrica
- **Producing Color**: Amarelo (`#FFFF00` com alpha 0.5)
- **Production Effect Prefab**: (Opcional) Efeito de partículas

#### Passo 3: Testar

1. Entre no Play Mode
2. Clique no botão de fábrica
3. Posicione o ghost onde desejar (igual outras estruturas)
4. Confirme com clique esquerdo
5. Após construção, a fábrica começará a produzir operários automaticamente
6. Operários aparecem em círculo ao redor da fábrica

**Visualização no Editor:**
- Selecione a fábrica na Scene View
- Um círculo verde mostra a área de spawn dos operários
- Cruz amarela marca o centro da fábrica

---

## 3. INTEGRAÇÃO COM SISTEMA EXISTENTE

### PlacerManager
Os scripts já estão integrados com o `PlacerManager` existente:
- Usam `PlacerManager.Instance.currentRecruitment` para recursos
- Chamam `PlacerManager.UpdateRecruitmentUI()` automaticamente
- Colocam unidades no `unitsContainer` se configurado

### Balanceamento Recomendado

**Recursos Iniciais:**
- `PlacerManager.currentRecruitment`: `100` (permite 3 recrutamentos em massa + sobra)

**Custos Sugeridos:**
- Recrutamento em Massa (x3 operários): `30` total (10 por operário)
- Produção Automática: `5` por operário (mais barato, mas lento)

**Timing da Fábrica:**
- Intervalo: `10s` = 6 operários/minuto
- Custo 5/unidade = 30 recursos/minuto

**Posicionamento Automático:**
- Os 3 operários spawnam em círculo ao redor do ponto clicado
- Distância entre unidades: `0.8f` (editável em `PlacerManager.RequestPlaceMultiple()`)

---

## 4. TESTES E VALIDAÇÃO

### Checklist de Teste

**Recrutamento em Massa:**
- [ ] Ghost aparece ao clicar no botão
- [ ] Ghost segue o mouse normalmente
- [ ] Clique esquerdo spawna exatamente 3 operários em círculo
- [ ] Clique direito ou ESC cancela o posicionamento
- [ ] Recursos diminuem em 30 (ou custo configurado)
- [ ] UI atualiza corretamente
- [ ] Sistema de build delay funciona para os 3 operários

**Produção Automática:**
- [ ] Fábrica pode ser posicionada com ghost preview
- [ ] Fábrica constrói normalmente (com build delay)
- [ ] Após construção, produção inicia automaticamente
- [ ] A cada 10s, spawna 1 operário ao redor da fábrica
- [ ] Efeito visual (flash amarelo) aparece ao produzir
- [ ] Para de produzir quando recursos < 5 (se requiresResources ativo)
- [ ] Operários aparecem em círculo ao redor da fábrica
- [ ] Gizmos mostram área de spawn na Scene View

### Debug
Ambos scripts têm logs detalhados. Abra o Console para ver:
- `[MassRecruitmentButton] Recrutando 3 operários...`
- `[FactoryProduction] Operário produzido! Total: X`

---

## 5. CUSTOMIZAÇÃO E VARIAÇÕES

### Variações Possíveis

**Recrutamento em Massa:**
```csharp
// No UnitCardUI, ajuste:
public int unitsPerPlacement = 3; // Altere para 5, 10, etc.

// Para diferentes unidades com diferentes quantidades:
// Card de Operário: unitsPerPlacement = 3
// Card de Soldado: unitsPerPlacement = 1 (individual)
// Card de Tanque: unitsPerPlacement = 1
```

**Produção da Fábrica:**
```csharp
// Controle dinâmico
factoryProduction.StopProduction();    // Para produção
factoryProduction.StartProduction();   // Inicia produção
factoryProduction.ToggleProduction();  // Alterna

// Informações em tempo real
float progress = factoryProduction.GetProductionProgress(); // 0-1
float timeLeft = factoryProduction.GetTimeUntilNextProduction(); // segundos

// Ajustar raio de spawn
public float spawnRadius = 1.5f; // Aumentar para área maior (ex: 2.5f)
```

### UI de Progresso da Fábrica (Opcional)

Crie um `Image` com `Fill Type: Filled` para mostrar progresso:

```csharp
// Em um script de UI
void Update()
{
    progressBar.fillAmount = factoryProduction.GetProductionProgress();
    timeText.text = $"{factoryProduction.GetTimeUntilNextProduction():F1}s";
}
```

---

## 6. TROUBLESHOOTING

### Problema: Ghost não aparece
- Verifique se `Ghost Prefab` está configurado no `UnitCardUI`
- Confirme que o ghost tem o componente `GhostFollower`
- Veja o Console para erros

### Problema: Spawna apenas 1 operário ao invés de 3
- Verifique se `Units Per Placement` está configurado como `3` no `UnitCardUI`
- Confirme que o método `RequestPlaceMultiple()` está sendo chamado

### Problema: Fábrica não produz
- Verifique se `requiresResources` está desmarcado OU se há recursos suficientes
- Confirme que `autoStartProduction` está marcado
- Veja se `Worker Prefab` está configurado
- Certifique-se que a fábrica foi **construída** (passou pelo build delay)

### Problema: Operários spawnam dentro da fábrica
- Aumente o `Spawn Radius` no FactoryProduction (padrão: 1.5)
- Valores sugeridos: 1.5 para fábricas pequenas, 2.5 para grandes

### Problema: Operários spawnam muito próximos/sobrepostos
- Ajuste `offsetDistance` em `PlacerManager.RequestPlaceMultiple()` (padrão: 0.8f)
- Aumente o valor para mais espaçamento (ex: 1.2f)
- Diminua para unidades menores (ex: 0.5f)

### Problema: Recursos não diminuem
- Confirme que `PlacerManager.Instance` não é null
- Verifique se `PlacerManager.UpdateRecruitmentUI()` está sendo chamado

---

## 7. PRÓXIMOS PASSOS

### Melhorias Futuras
1. **UI melhorada**: Adicionar ícones, animações
2. **Som**: SFX ao recrutar/produzir
3. **Efeitos visuais**: Partículas, animações de spawn
4. **Upgrades**: Fábrica produz mais rápido com upgrades
5. **Limite de população**: Máximo de unidades ativas

### Integração com Wave System
Você pode pausar/retomar produção baseado em waves:
```csharp
// No WaveManager
OnWaveComplete += () => factoryProduction.StartProduction();
OnWaveStart += () => factoryProduction.StopProduction();
```

---

**Scripts prontos para uso! Configure no Unity e teste.** 🚩
