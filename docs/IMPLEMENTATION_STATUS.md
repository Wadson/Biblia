# Status de implementação

## FASE 17 — Design System e Dashboard

Status: Concluída

### Implementado

- Paleta semântica para claro e escuro.
- Estilos reutilizáveis para títulos, campos, referências e cards.
- Dashboard alimentado por versão ativa e contadores reais.
- Atalhos para módulos futuros permanecem desabilitados até suas rotas existirem.

### Testes

- `dotnet test Tests\Biblia.Tests\Biblia.Tests.csproj --nologo --no-restore`: 15 aprovados.

### Build

- `dotnet build Biblia.slnx --nologo --no-restore -m:1`: OK, quatro targets MAUI.

### Próxima fase

FASE 18 — Leitor Bíblico.

## FASES 23–27 — Mensagens, tópicos, vínculos e editor

Status: Concluída

### Implementado

- CRUD de mensagens com busca e filtros por tipo.
- Tópicos persistidos, reordenados transacionalmente e acessíveis por página própria.
- Vínculos de referências com tópico opcional e observação contextual independente.
- Reordenação transacional de vínculos de referências.
- Duplicação transacional de mensagens, tópicos e vínculos, com remapeamento de tópicos.
- Editor responsivo com introdução, conclusão e versão preferida da mensagem.
- Migração versionada do banco do usuário, sem alteração das bases bíblicas somente leitura.

### Testes

- `dotnet test Tests\Biblia.Tests\Biblia.Tests.csproj --nologo --no-restore`: 15 aprovados.

### Build

- Windows: OK após os ajustes de reordenação.

### Próxima fase

FASE 28 — Pesquisa global.

## FASE 28 — Pesquisa global

Status: Concluída

### Implementado

- Busca por Bíblia, temas, referências, mensagens, tópicos e observações contextuais.
- Resultados organizados por domínio e ordenados para leitura.
- Cancelamento da consulta anterior quando uma nova busca é iniciada.
- Texto bíblico incluído quando há versão ativa instalada e habilitada.

### Validação

- `dotnet test Tests\Biblia.Tests\Biblia.Tests.csproj --nologo --no-restore`: 15 aprovados.
- Build Windows: 0 avisos e 0 erros.

### Próxima fase

FASE 29 — Relatórios.

## FASE 29 — Relatórios

Status: Concluída

### Implementado

- Modelo de relatório independente da UI para mensagens, tópicos e referências.
- Panorama de mensagens, pregações, estudos, devocionais, temas, referências e referências comentadas.
- Prévia de mensagem com introdução, tópicos, passagens disponíveis offline, comentário permanente e observação contextual.
- Página responsiva de relatórios acessível pelo dashboard e pela navegação principal.

### Próxima fase

FASE 30 — PDF.

## Auditoria de UI mobile e padronização

Status: Concluída em código e validação estática.

- Todas as telas registradas em `docs/UI_MOBILE_AUDIT.md` receberam revisão responsiva.
- Os menus `Picker` das Views foram substituídos por seletores temáticos com `CollectionView`.
- O leitor persiste o tamanho do texto dos versículos e mantém ações e intervalos utilizáveis em largura reduzida.
- A validação em aparelho físico/emulador permanece recomendada, pois não havia dispositivo controlável neste ambiente.

## Correção do leitor bíblico — seletores sincronizados

Status: Concluída em código e validação automatizada.

- Versão, livro e capítulo usam listas horizontais compactas, estado selecionado e `ScrollTo` visual após qualquer alteração do ViewModel.
- Anterior/Próximo atravessa livros quando existe livro adjacente e atualiza os dois seletores.
- Dois toques nos versículos definem o intervalo; a referência e o destaque são sincronizados sem listas permanentes de dezenas de números.
- O status técnico de quantidade de registros foi removido.
- A troca de tradução mantém livro, capítulo e intervalo por coordenadas canônicas, sem correlacionar por identificador interno de versículo.
- Os handlers usam SelectionChangedEventArgs e a sincronização de intervalo não executa duas vezes para o mesmo toque.

## Paleta visual de temas

Status: Concluída em código.

- `ThemeColorPickerView` fornece paleta visual de 21 cores e realce de seleção por borda.
- `Theme.ColorHex` continua persistido; cores históricas são preservadas como “Cor personalizada”.
- O modo novo remove contadores e exclusão; o modo edição solicita confirmação inline antes de excluir.
## Correção final da seleção e abertura da comparação

**Status:** implementada e validada por build/testes automatizados.

- Seleção visual controlada pelo ViewModel com `VerseItems`, comando de toque e refresh central.
- `CollectionView Multiple`, `SelectedItems.Clear()` e visual state nativo removidos do leitor.
- Comparação aberta por `IAppNavigator`, rota única e parâmetros canônicos.
- `BibleComparisonPage` inicializada por `IQueryAttributable`, com comparação automática e cards responsivos.
- DI de `BibleComparisonViewModel` e `BibleComparisonPage` confirmado em `AddPresentation()`.
# Módulo Mensagens — concluído em 2026-08-19

- [x] Filtro funcional por tipo e busca.
- [x] Criação/edição com `PreferredBibleVersionId`.
- [x] Bíblia integrada com versões instaladas/habilitadas, livros, capítulos e versículos reais.
- [x] Intervalo customizado, múltiplos temas, tópico e observação contextual.
- [x] Reutilização canônica e união de temas.
- [x] Criação, edição, listagem e remoção isolada de `MessageReference`.
- [x] Proteção contra vínculo duplicado na mesma mensagem/tópico.
- [x] Regressão: `BibleReaderPage` permaneceu inalterada.
# Tema global Bíblia Tema Premium — concluído em 2026-08-19

- [x] Paleta exata `Bt*` e aliases semânticos centralizados.
- [x] Estilos globais para páginas, textos, cards, botões, inputs, chips, menus, status e badges.
- [x] Estados Normal, Selected, Pressed e Disabled.
- [x] Flyout Premium e família de ícones SVG.
- [x] Light/Dark centralizados.
- [x] Todas as Views e componentes cobertos por estilos globais.
- [x] Selection sheet, alert, confirmação e loading customizados disponíveis.
- [x] Leitor e módulo Mensagens preservados funcionalmente.

# Adicionar referências — modernização funcional (2026-08-20)

- [x] Contexto da mensagem e filtros bíblicos compactos.
- [x] Versículos reais com toque único customizado, limpeza total e intervalo ordenado.
- [x] Tema único por operação em sheet pesquisável; tópico opcional em sheet customizado.
- [x] Comentário permanente separado da observação contextual.
- [x] `CanExecute` exige mensagem persistida, versão, livro, capítulo, intervalo e tema.
- [x] Reutilização canônica e união de temas anteriores com o novo tema.
- [x] Cards de vínculos com edição e remoção isolada de `MessageReference`.
- [x] Layout mobile empilhado e tablet/desktop em duas áreas.
- [x] Build completo sem avisos/erros e 23/23 testes automatizados aprovados.
# Home devocional

- Versículo diário persistente: implementado.
- Cinco categorias e fallback canônico: implementado.
- Texto carregado do banco bíblico ativo: implementado.
- Navegação para capítulo e atalhos: implementado.
