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
- Campos: mensagem, versão, livro, capítulo, intervalo, tópico e observação contextual.
- Mobile: formulário vertical com pares versão/livro e início/fim.
- Desktop: o mesmo formulário mantém pares de campos e lista de vínculos.
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
