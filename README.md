# Red Grid: Rise of the Comrades

## 📋 Sobre o Projeto

Jogo de estratégia tático para PC com arte pixel art.

**Engine:** Unity 6 (2025) LTS  
**Estilo:** 2D Pixel Art, câmera isométrica  
**Gênero:** Strategy / Tower Defense / Auto-battler  

---

## 🎮 Conceito

- **Placement Phase:** Posicionamento ilimitado de unidades e estruturas
- **Combat Phase:** Combate auto-resolve com ondas de inimigos
- **Recursos:** Sistema único de Recrutamento, Moral e Instabilidade
- **Runs:** Ataque (conquistar objetivos) e Defesa (segurar posições)

---

## 🛠️ Configuração do Projeto

### Requisitos:
- Unity 6 LTS (2025)
- Git instalado
- Windows/Mac/Linux

### Instalação:

1. Clone o repositório:
```bash
git clone https://github.com/RedFootGames/red-grid
```

2. Abra o projeto no Unity Hub:
   - `Add > Selecione a pasta "Red Grid"`
   - Versão: Unity 6 LTS

3. Aguarde o Unity importar os assets (pode demorar alguns minutos)

---

## 📁 Estrutura do Projeto

```
Assets/
├── Prefabs/          # Prefabs de unidades, estruturas, UI
│   ├── unit_militia_placeholder.prefab       # ComradeRecruit
│   ├── unit_operario_placeholder.prefab      # WorkerBrigade
│   ├── enemy_basic_placeholder.prefab        # BasicEnemy
│   └── HealthBarCanvas.prefab                # UI de HP
├── Scenes/           # Cenas do jogo
│   └── PlacementTest.unity  # Cena de teste do sistema de placement
├── Scripts/          # Scripts C#
│   ├── Placement/    # Sistema de posicionamento
│   │   ├── GhostFollower.cs
│   │   ├── UnitCardUI.cs
│   │   └── BuildProgressBar.cs
│   ├── Units/        # Sistema de unidades e combate
│   │   ├── UnitBase.cs           # Classe base abstrata
│   │   ├── ComradeRecruit.cs     # Infantaria aliada
│   │   ├── WorkerBrigade.cs      # Tank aliado
│   │   └── BasicEnemy.cs         # Inimigo básico
│   ├── UI/           # Interface de usuário
│   │   └── HealthBar.cs          # Barras de vida
│   ├── Debug/        # Ferramentas de debug
│   │   ├── CombatTester.cs       # Spawner de teste
│   │   └── UnitDebugger.cs       # Visualização de debug
│   └── PlacerManager.cs
├── Sprites/          # Sprites pixel art
├── UI/               # Elementos de UI
└── Settings/         # Configurações (URP, Input, etc)
```

---

## 🎯 Estado Atual (MVP - Fase 1)

### ✅ Implementado:
- [x] Sistema de placement (drag & drop)
- [x] Ghost preview com validação de colisão
- [x] Sistema de construção com barra de progresso
- [x] UI básica (botões de unidades, texto de recrutamento)
- [x] PlacerManager (gerenciamento de recursos - 50 pontos iniciais)
- [x] Câmera isométrica (Cinemachine)
- [x] Tags e Layers configurados (Units, Enemies, Structures, PlayerCore)
- [x] **Sistema de combate com IA** ✨
  - [x] UnitBase com state machine (Idle/Moving/Attacking/Dead)
  - [x] Detecção automática de inimigos (Physics2D + LayerMask)
  - [x] Movimento automático com Rigidbody2D
  - [x] Sistema de ataque com cooldown
  - [x] Health/damage system com eventos
  - [x] Sistema de priorização de alvos (Closest/LowestHealth/HighestDamage) 🎯
  - [x] 4 unidades implementadas:
    - ComradeRecruit (aliado - infantaria)
    - WorkerBrigade (aliado - tanque)
    - BasicEnemy (inimigo - básico)
    - EnemyTank (inimigo - tanque pesado) ✨ NOVO
- [x] **Sistema de Ondas (Wave System)** 🌊
  - [x] WaveManager com spawn progressivo
  - [x] 3 waves configuráveis (fácil, média, difícil)
  - [x] PlayerCore com HP e detecção de destruição
  - [x] Sistema de vitória (todas waves derrotadas)
  - [x] Sistema de derrota (core destruído)
  - [x] Timer entre waves (10 segundos)
  - [x] WaveUI para informações em tempo real

### 🚧 Em Desenvolvimento:
- [ ] Corrigir visualização das health bars
- [ ] Mais tipos de unidades
- [ ] Estruturas com habilidades ativas
- [ ] Balanceamento de waves

### 📅 Próximos Passos:
- Sistema de pontuação
- Arte pixel art definitiva
- Moral e Instabilidade
- Líderes e cartas

---

## 🤝 Contribuindo

### Para a equipe:

1. **Clone o projeto** (veja acima)
2. **Crie uma branch** para sua feature:
   ```bash
   git checkout -b feature/nome-da-feature
   ```
3. **Faça commits** descritivos:
   ```bash
   git add .
   git commit -m "feat: adiciona sistema de X"
   ```
4. **Push** para o repositório:
   ```bash
   git push origin feature/nome-da-feature
   ```
5. **Crie um Pull Request** no GitHub

### Padrões:
- **Commits:** Use prefixos `feat:`, `fix:`, `docs:`, `refactor:`
- **Código:** C# com comentários em português
- **Cenas:** Salve sempre antes de commitar

---

## 📚 Documentação

- [GDD Completo](https://docs.google.com/document/d/1EWAGpJmRFijgyJ7zyK1wJJzKamnfXZGRU_ZONwNW67k/edit?tab=t.0#heading=h.1zui15deflmj)
- **[Sistema de Combate](COMBAT_SYSTEM.md)** - Documentação completa da IA e mecânicas de luta
- **[Sistema de Ondas](WAVE_SYSTEM_SETUP.md)** - Guia de configuração do WaveManager
- [Mecânicas](docs/MECHANICS.md) *(criar depois)*
- [Arte e Estilo](docs/ART_STYLE.md) *(criar depois)*

---

## 👥 Equipe

- **Leonardo Almeida** - Desenvolvedor Principal e Game Designer
- *(adicione os membros da equipe aqui)*

---

## 📄 Licença

*(Defina a licença do projeto - MIT, GPL, etc)*

---

## 🔗 Links Úteis

- [Unity Documentation](https://docs.unity3d.com/)
- [Git Guide](https://git-scm.com/book/en/v2)
