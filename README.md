# Red Grid: Rise of the Comrades

## 📋 Sobre o Projeto

Jogo de estratégia tático marxista-leninista para PC com arte pixel art inspirada em pósters soviéticos e realismo socialista.

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
git clone [URL_DO_REPOSITORIO]
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
├── Scenes/           # Cenas do jogo
│   └── PlacementTest.unity  # Cena de teste do sistema de placement
├── Scripts/          # Scripts C#
│   ├── Placement/    # Sistema de posicionamento
│   ├── GhostFollower.cs
│   ├── UnitCardUI.cs
│   └── BuildProgressBar.cs
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
- [x] PlacerManager (gerenciamento de recursos)
- [x] Câmera isométrica (Cinemachine)
- [x] Tags e Layers configurados

### 🚧 Em Desenvolvimento:
- [ ] Sistema de combate (auto-resolve)
- [ ] IA de inimigos
- [ ] Sistema de ondas
- [ ] Moral e Instabilidade
- [ ] Líderes e cartas

### 📅 Próximos Passos:
- Testar sistema de placement completo
- Implementar unidades com comportamento básico
- Criar sistema de ondas de inimigos
- Arte pixel art definitiva

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

- [GDD Completo](docs/GDD.md) *(criar depois)*
- [Mecânicas](docs/MECHANICS.md) *(criar depois)*
- [Arte e Estilo](docs/ART_STYLE.md) *(criar depois)*

---

## 👥 Equipe

- **[Seu Nome]** - Desenvolvedor Principal
- *(adicione os membros da equipe aqui)*

---

## 📄 Licença

*(Defina a licença do projeto - MIT, GPL, etc)*

---

## 🔗 Links Úteis

- [Unity Documentation](https://docs.unity3d.com/)
- [Git Guide](https://git-scm.com/book/en/v2)
