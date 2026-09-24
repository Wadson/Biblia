# Telas

## Ler Bíblia

- Rota: `//BibleReader`
- View / ViewModel: `BibleReaderPage` / `BibleReaderViewModel`
- Serviços: versões, repositório bíblico, comparação, referências salvas, temas, mensagens, área de transferência e configurações.
- Comandos: capítulo anterior/próximo, A−/A/A+, copiar, salvar referência, adicionar ao tema, comparar e limpar seleção.
- Campos: versão, livro, capítulo e intervalo de versículos.
- Mobile: seletores horizontais compactos (42 dp), com item atual marcado e rolagem interna; a leitura permanece como área principal. O intervalo é formado por dois toques nos versículos e exibido de forma resumida.
- Desktop: seletores e ações aproveitam a largura disponível e rolam automaticamente até versão, livro e capítulo ativos.
- Troca de versão preserva livro, capítulo e intervalo por referência canônica (livro/capítulo/versículo), quando a tradução de destino contém o trecho.
- A versão escolhida é persistida como versão ativa; destaque e auto-scroll pertencem somente à View.

## Mensagens e Pregações

## Temas

- Rota: `//Themes`
- `ThemesPage` / `ThemesViewModel` usam `ThemeColorPickerView`, uma paleta visual reutilizável de 21 cores.
- `Theme.ColorHex` continua persistido; cor legada fora da paleta aparece como “Cor personalizada” sem ser alterada.
- Novo tema não mostra contadores ou exclusão; edição usa confirmação inline antes de excluir.

- Rota: `//MessageBibleReferences`
- View / ViewModel: `MessageBibleReferencesPage` / `MessageReferencesViewModel`
- Serviços: mensagens, tópicos, vínculos, referências salvas, versões e repositório bíblico.
- Comandos: carregar, adicionar, salvar observação e remover vínculo.
- Campos: tema, versão, livro, capítulo, intervalo e comentário.
- Mobile: tema no topo; trecho bíblico e vínculo empilhados.
- Desktop: tema em faixa única; seletor bíblico à esquerda e vínculo/lista à direita.
## Correção final — leitor e comparação multi-versão

- A lista de versículos usa `SelectionMode=None`; cada toque executa `SelectVerseCommand` e o destaque depende exclusivamente de `BibleVerseItemViewModel.IsSelected`.
- Um segundo toque no único versículo selecionado limpa completamente o intervalo e o destaque. Após um intervalo concluído, um novo toque inicia uma nova seleção.
- **Comparar versões** navega para a rota interna `BibleComparison` com livro canônico, nome, capítulo, intervalo e versão de origem.
- A tela de comparação recebe a referência pela navegação, seleciona todas as versões instaladas/habilitadas e carrega os cards automaticamente, sem formulário manual.
- Os cards exibem número e texto de cada versículo, nome da versão, badge da versão atual e estados amigáveis de ausência, ambiguidade e erro.
# Mensagens — fluxo integrado

`MessagesPage` permite pesquisar e filtrar por Todas, Mensagem, Pregação, Estudo ou Devocional, além de criar/editar mensagens com versão bíblica preferida. Após salvar, a ação **Referências bíblicas** abre o fluxo integrado.

`MessageBibleReferencesPage` oferece versão, livro, capítulo, texto real dos versículos, seleção por toque de versículo/intervalo, múltiplos temas, tópico opcional, observação contextual e a lista de vínculos. Tocar novamente no único versículo selecionado limpa a seleção; o segundo toque distinto forma o intervalo.

### Adicionar referências — fluxo atual

- Ordem: mensagem, versão, livro, capítulo, versículos, tema único por operação, tópico opcional, comentário permanente, observação contextual e vínculo.
- Versículos usam `SelectionMode=None` e um único `TapGestureRecognizer`: um toque seleciona, repetir o mesmo toque limpa e dois números formam intervalo ordenado.
- Tema e tópico usam `AppSelectionSheet`; não há `Picker`, diálogo Android ou seleção múltipla nativa. A pesquisa de tema chama `IThemeService.SearchAsync` com cancelamento/debounce.
- `SavedReference.Comment` guarda o comentário permanente. `MessageReference.Observation` guarda apenas o contexto daquela mensagem.
- A chave canônica é `BookReferenceId + Chapter + VerseStart + VerseEnd`; referências existentes são reutilizadas e seus temas recebem união com o novo tema.
- Editar atualiza tema, tópico, comentário, observação e versão preferida sem duplicar a referência. Remover exclui somente `MessageReference`.
# Home — Versículo do Dia

A Home apresenta categoria, referência, texto real da versão bíblica ativa e versão utilizada. A referência é estável durante o dia e o botão **Ler capítulo inteiro** abre o leitor no livro e capítulo correspondentes. Os atalhos principais são Bíblia, Pesquisa, Temas, Mensagens, Referências e Relatórios.

# Mensagens e Pregações

Entrada única para selecionar ou criar uma mensagem/pregação, escolher o tema de trabalho, adicionar vários trechos com comentários, reorganizar a ordem, duplicar e abrir o relatório/PDF. Seções são opcionais e ficam no modo avançado.

# Configurações

Reúne aparência, resumo das versões instaladas, criação/exportação/compartilhamento/restauração de backup e painel Sobre. Operações de arquivo usam os seletores e o compartilhamento seguros do sistema operacional.
# Auditoria de navegação global — 20/08/2026

| Página | Tipo | Voltar | Motivo |
|---|---|---:|---|
| `MainPage` | ROOT | Não | Início do Flyout |
| `BibleReaderPage` | ROOT | Não | Bíblia no Flyout |
| `BibleSearchPage` | ROOT | Não | Pesquisar no Flyout |
| `BibleVersionsPage` | ROOT | Não | Versões no Flyout |
| `ThemesPage` | ROOT | Não | Temas no Flyout; criação/edição ocorre na própria página |
| `SavedReferencesPage` | ROOT | Não | Referências guardadas no Flyout |
| `MessageBibleReferencesPage` | ROOT | Não | Mensagens e Pregações no Flyout |
| `GlobalSearchPage` | ROOT | Não | Pesquisa global no Flyout |
| `ReportsPage` | ROOT | Não | Relatórios no Flyout |
| `SettingsPage` | ROOT | Não | Configurações no Flyout; Sobre é overlay local com Fechar |
| `BibleComparisonPage` | SECONDARY | Sim | Aberta a partir do leitor bíblico |
| `MessagesPage` | SECONDARY | Sim | Editor aberto pelo fluxo unificado |
| `MessageTopicsPage` | SECONDARY | Sim | Gerenciamento aberto a partir de mensagem |
| `MessageReferencesPage` | SECONDARY | Sim | Detalhes de vínculos de uma mensagem |
| `MessageDuplicationPage` | SECONDARY | Sim | Formulário auxiliar de duplicação |

As páginas secundárias usam o `PageHeaderView` reutilizável, com `← Voltar` no canto superior direito e o Back nativo oculto visualmente. A ação preserva a pilha real (`..`), é protegida contra toques concorrentes, é coerente com o gesto/botão Android e não redireciona para Início. Páginas ROOT mantêm o hambúrguer do Flyout.
# Relatórios — gerador profissional

`ReportsPage` permite gerar Pregação ou Mensagem a partir de uma mensagem cadastrada ou de um conjunto de temas. A configuração inclui título, subtítulo, introdução, conclusão e versão bíblica; a prévia é estruturada por seções e referências. Após gerar o PDF, um modal oferece **Salvar em...**, **Compartilhar** e **Cancelar**. Em mobile os painéis são empilhados; em desktop configuração e prévia ocupam colunas.
# Compartilhar uma Palavra

A parte superior da Home e o Versículo do Dia permanecem inalterados. A área inferior prioriza **Compartilhar uma Palavra**, com botão **Criar card** e atalhos compactos para Bíblia e Pesquisar. `VerseCardStudioPage` é uma rota secundária com Voltar, prévia reativa, escolha bíblica real, tema, saudação, formato, template, fundos online/offline, geração PNG, salvar e compartilhar.
