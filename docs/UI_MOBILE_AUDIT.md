# Auditoria de UI mobile e padronização

## Inventário inicial

| Tela | Rota | Problemas identificados | Mobile | Desktop | Controles nativos | Status |
|---|---|---|---|---|---|---|
| Início | `//Home` | cartões e indicadores em grids fixos; cores locais | 2 cartões por linha no acesso rápido | preservar painéis | nenhum | concluído em código |
| Leitor bíblico | `//BibleReader` | seletores compactos, selecionados e sincronizados; intervalo reduzido | listas horizontais de 42 dp e leitura prioritária | rolagem automática até o item ativo | nenhum Picker | concluído em código |
| Pesquisa bíblica | `//BibleSearch` | filtros horizontais | painel de filtros | lista ampla | CheckBox | concluído em código |
| Comparação | `//BibleComparison` | campos comprimidos corrigidos | campos em pares e ação própria | cards de versões | CheckBox | concluído em código |
| Versões | `//BibleVersions` | ações por card corrigidas | cards verticais e ações em pares | cards largos | FilePicker legítimo | concluído em código |
| Referências salvas | `//SavedReferences` | formulário em duas colunas | empilhar | lista + detalhes | nenhum Picker | concluído em código |
| Temas | `//Themes` | lista + editor fixos | empilhar | preservar | nenhum Picker | concluído em código |
| Mensagens e Pregações | `//MessageBibleReferences` | seleção bíblica e vínculos | listas roláveis, intervalo em 2 colunas | formulário amplo | nenhum Picker | concluído em código |
| Tópicos / referências | rotas próprias | grids de duas/três colunas | empilhar | manter painéis | nenhum Picker | concluído em código |
| Pesquisa global | `//GlobalSearch` | sem filtros visuais | painel | painel | nenhum | concluído em código |
| Relatórios | `//Reports` | painel adaptativo inicial | cards empilhados | painel | nenhum Picker | concluído em código |
| Configurações | `//Settings` | conteúdo inicial incompleto | cards | cards | Switch | concluído em código |

## Decisões

- `FilePicker` permanece integração legítima de sistema operacional.
- Todos os `Picker` das Views de produção foram substituídos por seletores explícitos com `CollectionView`, preservando o binding e os comandos dos respectivos ViewModels.
- CheckBoxes e Switches continuam sendo controles nativos adequados para seleção múltipla e preferências booleanas; não representam menus de escolha.
- A validação visual em aparelhos reais ainda depende de emulador/dispositivo disponível; não há evidência de screenshots registrada.

## Implementações desta etapa

- Leitor: A−/A/A+ altera somente o texto dos versículos e persiste `BibleReaderFontSize`.
- Leitor: versão, livro e capítulo ativos têm estado selecionado e a View executa `ScrollTo` centralizado após mudanças do ViewModel ou navegação anterior/próximo.
- Leitor: a navegação atravessa os limites de capítulo para o livro adjacente, quando disponível, mantendo livro e capítulo sincronizados.
- Leitor: intervalo foi reduzido a uma faixa resumida; dois toques na leitura definem início/fim e destacam discretamente os versículos do intervalo.
- Leitor: a troca de versão preserva a referência e o intervalo canônicos quando disponíveis, e o primeiro toque já seleciona um único versículo.
- Leitor: handlers leem o item de SelectionChangedEventArgs e o destaque de versículos é sincronizado apenas pelo PropertyChanged, sem chamada duplicada.
- Temas: seleção de cor deixou de exibir hexadecimal e usa `ThemeColorPickerView`, com 21 swatches, preview e preservação de cores legadas.
- Leitor: ação de adicionar referência à mensagem removida da UI principal.
- Mensagens e Pregações: rota separada cria/reutiliza referência canônica e mantém observação contextual no vínculo.
- Leitor e Mensagens/Pregações: os seletores principais foram substituídos por `CollectionView` temático, evitando diálogos nativos de lista.
- Comparação: quatro entradas foram convertidas em campos em pares, eliminando a linha comprimida de cinco controles.
- Versões: ações foram convertidas de uma faixa horizontal fixa para botões que quebram em pares.
- Mensagens: ações de salvar/excluir passaram a quebrar em pares em telas estreitas.
- Referências salvas, temas, tópicos, referências, pesquisa, relatórios e configurações foram reorganizados com `FlexLayout`/pilhas roláveis para largura reduzida.
- Ações de seleção de versão, livro, capítulo, versículo, intervalo, tema e tipo de mensagem passaram a ter alternativa visível e tocável sem abrir um diálogo nativo de lista.

## Validação

- Build final executado para Android, iOS, MacCatalyst e Windows: sem erros ou avisos.
- Testes automatizados finais: `15/15` aprovados.
- Revisão estática: nenhuma chamada a `Picker`, `DisplayAlert` ou `DisplayActionSheet` foi encontrada nas Views.
- Light/Dark: as Views auditadas usam os recursos semânticos existentes (`AppCard`, estilos de botão, textos e campos); a paleta local histórica do dashboard permanece centralizada em `MainPage.xaml`.
- Small phone (360 dp): os painéis novos usam `FlexLayout` com `Wrap` e base de 48%, e os formulários críticos passam a uma coluna.

## Limites de evidência

Não há emulador/dispositivo controlável neste ambiente para capturar telas ou validar interação tátil/leitor de tela. Essas evidências não foram inventadas; recomenda-se a confirmação manual em Android físico ou emulador de 360 dp antes de publicação.
## Leitor e comparação — checkpoint final

- Removida a seleção múltipla nativa dos versículos, evitando estado visual reciclado ou residual.
- O template usa gatilhos vinculados a `IsSelected`, com fundo transparente e borda zero no estado limpo.
- A comparação usa uma única `CollectionView` principal; os versículos dentro de cada card usam `BindableLayout`, sem alturas fixas nem listas roláveis aninhadas.
- A referência é exibida como nome canônico e intervalo (por exemplo, `Gênesis 1:26–28`) e não exige digitação no mobile.
# Auditoria mobile — Mensagens (2026-08-19)

- Chips de tipo, versão, livro, capítulo, temas e tópicos têm rolagem horizontal própria e seleção explícita.
- A página não depende de `Picker` nativo nem de seleção múltipla nativa para intervalos.
- Os versículos ocupam a largura do painel, com número e texto em linhas tocáveis; estado selecionado possui fundo e contorno próprios.
- O botão de adicionar ocupa a largura disponível e só habilita com mensagem persistida, versão, livro, capítulo, intervalo e ao menos um tema.
- A lista de vínculos apresenta referência, temas, tópico e observação sem duplicar texto bíblico no banco do usuário.
# Auditoria do Design System (2026-08-19)

- Paleta: fundo `#F8FAFC`, superfície branca, texto primário `#0F172A`, secundário `#64748B`, borda `#E2E8F0`, seleção `#E0E7FF` e accent `#6366F1`.
- Cards possuem raio de 12px e espaçamento consistente; botões oferecem estados hover e pressed.
- O leitor usa cabeçalho compacto, seletores horizontais, área de leitura ampla e barra contextual inferior.
- Mensagens usa cards de uma coluna no painel de lista e reorganização por `FlexLayout` no desktop.
- Configurações foi agrupada em Aparência, Versões instaladas, Backup e restauração e Sobre.
# Auditoria global Premium (2026-08-19)

- Todas as páginas herdam `BtPageStyle`, fundo off-white e equivalentes Dark.
- Alvos interativos têm mínimo de 48dp; cards usam raio 12–14 e listas evitam seleção nativa escura.
- Em 360/390dp, `FlexLayout` quebra painéis para uma coluna; tablet e desktop usam largura máxima legível.
- Seletores de Bíblia continuam horizontais e não provocam scroll horizontal da página.
- Inputs, botões, checks, radios, switches e sliders recebem estilos globais consistentes.
- Flyout usa header azul-marinho, marca branca/dourada, SVGs azuis e seleção azul-claro.
- `Theme.ColorHex` permanece acento de dados do usuário e não é substituído pelo tema global.

# Adicionar referências — auditoria de interação (2026-08-20)

- Removida a combinação de seleção nativa e gesto nos versículos: o card inteiro usa somente `TapGestureRecognizer`, com estado proveniente de `IsSelected`.
- Versão, livro e capítulo mantêm listas horizontais de altura controlada e auto-scroll; mudar livro/capítulo limpa integralmente o intervalo anterior.
- Tema e tópico são campos de seleção de 48dp e abrem sheets sobrepostos com `ZIndex` explícito; o overlay só existe enquanto o sheet está visível.
- O formulário permanece rolável com teclado aberto; listas verticais internas têm altura definida, evitando disputa ilimitada entre `ScrollView` e `CollectionView`.
- Em largura estreita, Bíblia, vínculo e referências ficam empilhados e full width. A partir de 800dp, Bíblia e vínculo formam duas colunas e a lista ocupa a largura inferior.
- Estados normal/selecionado e disabled usam somente tokens `Bt*`; não há `Picker`, `DisplayActionSheet` ou diálogo visual nativo.
