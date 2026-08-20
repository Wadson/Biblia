# Registro de decisões e checkpoints

## Decisões iniciais

- Manter o projeto MAUI single-project e separar responsabilidades por pastas/namespaces.
- Manter testes puros de domínio em projeto separado, sem dependência do runtime MAUI.
- Usar logging nativo de `Microsoft.Extensions.Logging`.
- Tratar `Versoes` como fonte imutável. NAA, NTLH e NVT permanecem incompatíveis e não serão corrigidos no local.

## Fase 0

```text
FASE: 0 — Auditoria do Projeto
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: docs/AUDITORIA_PROJETO.md
ARQUIVOS ALTERADOS: nenhum arquivo de código
TESTES EXECUTADOS: dotnet build Biblia.slnx
RESULTADO: sucesso, quatro targets compilados
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica
PRÓXIMA FASE: 1 — Auditoria das Bíblias
```

## Fase 1

```text
FASE: 1 — Auditoria das Bíblias
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: docs/BANCOS_BIBLICOS.md
ARQUIVOS ALTERADOS: nenhum banco SQLite
TESTES EXECUTADOS: quick_check, foreign_key_check e validações estruturais/referenciais
RESULTADO: 10 versões classificadas; 3 incompatíveis documentadas
WARNINGS: diferenças legítimas de versificação em versões classificadas com ressalva
PENDÊNCIAS: NAA, NTLH e NVT não podem ser usadas como fontes confiáveis no estado atual
PRÓXIMA FASE: 2 — Fundação da Arquitetura
```

## Fase 2

```text
FASE: 2 — Fundação da Arquitetura
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: AGENTS.md; Domain/*; Application/*; Infrastructure/*; Presentation/*; Tests/Biblia.Tests/*; docs/ARQUITETURA.md; docs/DECISOES.md
ARQUIVOS ALTERADOS: Biblia.slnx; Biblia.csproj; MauiProgram.cs; App.xaml.cs
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj; dotnet build Biblia.slnx
RESULTADO: 5/5 testes aprovados; quatro targets MAUI e projeto de testes compilados
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da fundação; versões NAA, NTLH e NVT continuam incompatíveis e isoladas
PRÓXIMA FASE: 3 — Banco do Aplicativo
```

## Fase 3

```text
FASE: 3 — Banco do Aplicativo
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: Domain/Entities/*; Domain/Enums/*; Application/Interfaces/IAppDatabase.cs; Application/Interfaces/Repositories/*; Infrastructure/AppDatabase/AppDatabase.cs; Infrastructure/Repositories/*; Tests/Biblia.Tests/Integration/AppDatabaseTests.cs; docs/MODELO_DADOS.md
ARQUIVOS ALTERADOS: Biblia.csproj; Infrastructure/DependencyInjection.cs; Tests/Biblia.Tests/Biblia.Tests.csproj
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj; dotnet build Biblia.slnx
RESULTADO: schema v1 e migration idempotente; CRUD, constraints, FKs e cascatas validados; 6/6 testes aprovados; quatro targets MAUI compilados
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 3
PRÓXIMA FASE: 4 — Sistema de Versões
```

## Fase 4

```text
FASE: 4 — Sistema de Versões
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: Domain/Entities/BibleValidationResult.cs; Domain/Entities/BibleVersionManifest.cs; Application/Interfaces/IBibleVersion*.cs; Application/Services/BibleVersionManager.cs; Infrastructure/BibleDatabases/BibleValidationService.cs; Infrastructure/Files/MauiBibleVersionManifestProvider.cs; Resources/Raw/bible-versions.manifest.json; testes de validação e gerenciador
ARQUIVOS ALTERADOS: Application/DependencyInjection.cs; Infrastructure/DependencyInjection.cs; catálogo e projeto de testes
TESTES EXECUTADOS: validação de banco compatível/duplicado; catálogo idempotente; ativação e resolução; dotnet test; dotnet build
RESULTADO: 8/8 testes aprovados; catálogo, manifesto, versão padrão, ativação e validação implementados
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 4
PRÓXIMA FASE: 5 — Empacotamento das Bíblias
```

## Fase 5

```text
FASE: 5 — Empacotamento das Bíblias
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: Resources/Raw/Bibles/{ACF,ARA,ARC,AS21,KJF,NBV,NVI}.sqlite; Application/Services/InitialBibleInstallationService.cs; Application/Services/AppInitializationService.cs; Infrastructure/Files/MauiPackagedBibleSource.cs; teste de instalação
ARQUIVOS ALTERADOS: App.xaml.cs; registros de DI
TESTES EXECUTADOS: cópia, hash, validação, registro, versão padrão e reinstalação idempotente; inspeção de assets Android; dotnet test; dotnet build
RESULTADO: 7 versões aprovadas incorporadas; 9/9 testes aprovados; assets presentes no pacote; inicialização automática configurada
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 5
PRÓXIMA FASE: 6 — Importação Manual
```

## Fase 6

```text
FASE: 6 — Importação Manual
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: BibleVersionImportService; MauiBibleFilePicker; tela e ViewModel de versões; comandos assíncronos; testes de importação
ARQUIVOS ALTERADOS: AppShell; App; MauiProgram; DI
TESTES EXECUTADOS: banco válido, colisão, corrompido, cópia privada, preservação do original, remoção; dotnet test; dotnet build
RESULTADO: 10/10 testes aprovados; tela funcional acessível pelo Shell; importação nunca usa nem altera permanentemente o arquivo externo
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 6
PRÓXIMA FASE: 7 — Bible Repository
```

## Fase 7

```text
FASE: 7 — Bible Repository
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: BibleBook.cs; BibleVerse.cs; BiblePassage.cs; IBibleRepository.cs; BibleRepository.cs; BibleRepositoryTests.cs
ARQUIVOS ALTERADOS: Infrastructure/DependencyInjection.cs; projeto de testes
TESTES EXECUTADOS: livros, capítulos, versículos, passagem, duas traduções com IDs sequenciais diferentes e duplicidade explícita; dotnet test; dotnet build
RESULTADO: 11/11 testes aprovados; consultas somente leitura e parametrizadas; correlação exclusivamente por referência canônica
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 7
PRÓXIMA FASE: 8 — Leitor Bíblico
```

## Fase 8

```text
FASE: 8 — Leitor Bíblico
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: BibleReaderPage; BibleReaderViewModel; IClipboardService; MauiClipboardService; teste do ViewModel
ARQUIVOS ALTERADOS: AppShell; DI; MessageRepository; layout responsivo
TESTES EXECUTADOS: carregamento, troca de versão preservando referência, intervalo, cópia, salvamento, tema, mensagem e comparação; execução visual real no Windows; dotnet test; dotnet build
RESULTADO: 12/12 testes aprovados; ACF/Gênesis 1 exibido com 31 versículos no aplicativo real; controles com quebra adaptativa para largura compacta
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 8
PRÓXIMA FASE: 9 — Pesquisa Bíblica
```

## Fase 9

```text
FASE: 9 — Pesquisa Bíblica
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: BibleSearchQuery.cs; BibleSearchResult.cs; IBibleSearchService.cs; BibleSearchService.cs; teste multi-versão
ARQUIVOS ALTERADOS: IBibleRepository; BibleRepository; DI
TESTES EXECUTADOS: palavra/expressão parametrizada, filtros, paginação, limite, múltiplas versões; medição real nas 7 bases; dotnet test; dotnet build
RESULTADO: 13/13 testes aprovados; consultas reais de “amor” entre 45,26 ms e 96,90 ms por versão, limitadas a 100 resultados
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 9
PRÓXIMA FASE: 10 — Comparação
```

## Fase 10

```text
FASE: 10 — Comparação
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: BibleComparisonStatus.cs; BibleComparisonItem.cs; IBibleComparisonService.cs; BibleComparisonService.cs; teste de divergências
ARQUIVOS ALTERADOS: BibleReaderViewModel; DI
TESTES EXECUTADOS: referência disponível, ausente e duplicada/ambígua; integração no leitor; dotnet test; dotnet build
RESULTADO: 14/14 testes aprovados; comparação não escolhe silenciosamente textos duplicados
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 10
PRÓXIMA FASE: 11 — Temas
```

## Fase 11

```text
FASE: 11 — Temas
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: Domain/Rules/ThemeRules.cs; Domain/Entities/ThemeDetails.cs; Application/Interfaces/IThemeService.cs; Application/Services/ThemeService.cs; Presentation/Views/ThemesPage.xaml; Presentation/Views/ThemesPage.xaml.cs; Presentation/ViewModels/ThemesViewModel.cs; Tests/Biblia.Tests/Application/ThemeServiceTests.cs
ARQUIVOS ALTERADOS: IThemeRepository; ThemeRepository; registros de DI; AppShell; MainViewModel; testes de persistência e projeto de testes
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
RESULTADO: 15/15 testes aprovados; build dos quatro targets MAUI e testes concluído sem erros; CRUD, validação de nome/cor, pesquisa e contagem de referências/mensagens relacionadas implementados; exclusão mantém integridade via chaves estrangeiras e cascata de ReferenceTheme.
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica da Fase 11.
PRÓXIMA FASE: 12 — Referências Salvas
```

## Fase 12

```text
FASE: 12 — Referências Salvas
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: BibleReferenceRange; SavedReferenceDetails; ISavedReferenceService; SavedReferenceService; SavedReferencesPage; SavedReferencesViewModel
ARQUIVOS ALTERADOS: ISavedReferenceRepository; SavedReferenceRepository; DI; AppShell; dashboard; testes de integração
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
RESULTADO: CRUD de referências canônicas, intervalos, comentário permanente, versão de visualização e associação transacional de temas implementados; 15/15 testes aprovados; quatro targets MAUI compilados.
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica.
PRÓXIMA FASE: 13 — Mensagens
```

## Fase 13

```text
FASE: 13 — Mensagens
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: IMessageService; MessageService; MessagesPage; MessagesViewModel
ARQUIVOS ALTERADOS: DI; AppShell; dashboard
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
RESULTADO: CRUD, tipos e pesquisa de mensagens implementados; 15/15 testes aprovados; quatro targets MAUI compilados.
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica.
PRÓXIMA FASE: 14 — Tópicos
```

## Fase 14

```text
FASE: 14 — Tópicos
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: IMessageTopicService; MessageTopicService; MessageTopicsPage; MessageTopicsViewModel
ARQUIVOS ALTERADOS: IMessageRepository; MessageRepository; DI; AppShell; testes de dublês
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
RESULTADO: criação, edição, exclusão e reordenação transacional de tópicos implementadas; 15/15 testes aprovados; quatro targets MAUI compilados.
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica.
PRÓXIMA FASE: 15 — Referências na Mensagem
```

## Fase 15

```text
FASE: 15 — Referências na Mensagem
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: IMessageReferenceService; MessageReferenceService; MessageReferencesPage; MessageReferencesViewModel
ARQUIVOS ALTERADOS: IMessageRepository; MessageRepository; DI; AppShell; testes de dublês
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
RESULTADO: vínculos de referências a mensagens/tópicos, observação contextual independente do comentário permanente, versão preferida e remoção implementados; 15/15 testes aprovados; quatro targets MAUI compilados.
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica.
PRÓXIMA FASE: 16 — Duplicação
```

## Fase 16

```text
FASE: 16 — Duplicação
STATUS: CONCLUÍDA
ARQUIVOS CRIADOS: IMessageDuplicationService; MessageDuplicationService; MessageDuplicationPage; MessageDuplicationViewModel
ARQUIVOS ALTERADOS: IMessageRepository; MessageRepository; DI; AppShell; teste de integração
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
RESULTADO: duplicação transacional cria nova mensagem, novos tópicos e novos vínculos, remapeia tópicos, preserva observações e reutiliza referências existentes; 15/15 testes aprovados; quatro targets MAUI compilados.
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica.
PRÓXIMA FASE: 17 — Pesquisa Global
```

## Fase 17 (prompt de continuação)

```text
FASE: 17 — Consolidar Design System e Dashboard
STATUS: CONCLUÍDA
IMPLEMENTADO: paleta semântica claro/escuro, estilos reutilizáveis, dashboard ligado a dados reais e atalhos de módulos futuros desabilitados.
ARQUIVOS ALTERADOS: Resources/Styles/Colors.xaml; Resources/Styles/Styles.xaml; MainPage.xaml
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore
BUILD: dotnet build Biblia.slnx --nologo --no-restore -m:1 — OK (Android, iOS, MacCatalyst e Windows)
WARNINGS: 0
PENDÊNCIAS: nenhuma crítica.
PRÓXIMA FASE: 18 — Refinar Leitor Bíblico
```

## Fase 27

```text
FASE: 27 — Editor de mensagem responsivo
STATUS: CONCLUÍDA
IMPLEMENTADO: migração v2 do banco do usuário para introdução, conclusão e versão bíblica preferida; persistência, atualização e duplicação desses campos; editor de mensagens com layout FlexLayout que se reorganiza em telas estreitas e lista apenas versões instaladas/habilitadas.
ARQUIVOS ALTERADOS: Message; AppDatabase; MessageRepository; IMessageService; MessageService; MessagesViewModel; MessagesPage; AppDatabaseTests.
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore — 15/15 aprovados.
BUILD: dotnet build Biblia.csproj -f net10.0-windows10.0.19041.0 --nologo --no-restore -m:1 — OK.
WARNINGS: 0
PENDÊNCIAS: a edição detalhada de tópicos e vínculos continua nas páginas próprias; a composição integrada de três painéis é melhoria futura, não é marcada como entregue.
PRÓXIMA FASE: 28 — Pesquisa global.
```

## Fase 28

```text
FASE: 28 — Pesquisa global
STATUS: CONCLUÍDA
IMPLEMENTADO: busca por Bíblia na versão ativa, temas, referências, mensagens, tópicos e observações contextuais; cancelamento de busca anterior no ViewModel e ordenação por domínio.
ARQUIVOS ALTERADOS: GlobalSearchService; GlobalSearchViewModel; IMPLEMENTATION_STATUS.
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore — 15/15 aprovados.
BUILD: dotnet build Biblia.csproj -f net10.0-windows10.0.19041.0 --nologo --no-restore -m:1 — OK.
WARNINGS: 0
PENDÊNCIAS: filtros avançados de versão, livro, tema, tipo e período serão consolidados na revisão da busca.
PRÓXIMA FASE: 29 — Relatórios.
```

## Fase 29

```text
FASE: 29 — Relatórios
STATUS: CONCLUÍDA
IMPLEMENTADO: IReportService, modelos independentes da UI, panorama de conteúdo e prévia profissional de mensagem com tópicos, texto bíblico disponível, comentários e observações.
ARQUIVOS CRIADOS: IReportService; ReportService; MessageReport; ReportsOverview; ReportsPage; ReportsViewModel.
ARQUIVOS ALTERADOS: DI de Application/Presentation; AppShell; MainViewModel.
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore — 15/15 aprovados.
BUILD: dotnet build Biblia.slnx --nologo --no-restore -m:1 — OK (Android, iOS, MacCatalyst e Windows).
WARNINGS: 0
PENDÊNCIAS: geração e compartilhamento de PDF pertencem à Fase 30.
PRÓXIMA FASE: 30 — PDF.
```

## Fases 30–31

```text
FASE: 30 — PDF
STATUS: CONCLUÍDA
IMPLEMENTADO: IPdfService e PdfService offline, A4 paginado, com margens, títulos, referências, passagens, comentários e observações; ação Salvar PDF na prévia de relatórios.
FASE: 31 — Backup e restauração
STATUS: PARCIAL
IMPLEMENTADO: IBackupService e BackupService com ZIP do banco do usuário, manifesto (data/schema), validação, backup preventivo e restauração atômica com recuperação em erro. Bases bíblicas não são incluídas.
PENDÊNCIAS: tela para escolher/confirmar arquivo de restauração e compartilhamento de PDF serão concluídos na fase de configurações/backup.
TESTES EXECUTADOS: dotnet test Tests/Biblia.Tests/Biblia.Tests.csproj --nologo --no-restore — 15/15 aprovados.
BUILD: dotnet build Biblia.slnx --nologo --no-restore -m:1 — em validação final após os novos serviços.
```

## Auditoria de UI mobile e padronização

```text
STATUS: CONCLUÍDA
IMPLEMENTADO: revisão de todas as rotas reais, layouts responsivos para telas estreitas, seletor bíblico e de formulários por CollectionView, controles de fonte A−/A/A+ persistentes no leitor e melhoria de semântica/estados vazios.
REGISTRO: docs/UI_MOBILE_AUDIT.md
VALIDAÇÃO: dotnet test Tests\Biblia.Tests\Biblia.Tests.csproj --nologo --no-restore; dotnet build Biblia.slnx --nologo --no-restore -m:1
EVIDÊNCIA DE DISPOSITIVO: não disponível neste ambiente; sem screenshots inventadas.
```

## Correção do leitor — seletores sincronizados

```text
STATUS: CONCLUÍDA
IMPLEMENTADO: seletores compactos de versão/livro/capítulo com estado selecionado e ScrollTo visual; navegação entre capítulos e livros adjacentes; intervalo por dois toques na leitura e faixa resumida; status técnico removido.
VALIDAÇÃO: dotnet build Biblia.slnx --nologo --no-restore -m:1 — OK (0 avisos, 0 erros); dotnet test Tests\Biblia.Tests\Biblia.Tests.csproj --nologo --no-restore — 15/15 aprovados.
EVIDÊNCIA VISUAL: nenhuma screenshot de dispositivo foi declarada, pois o ambiente não expõe emulador/aparelho controlável.
```

## Paleta visual de temas

```text
STATUS: CONCLUÍDA
IMPLEMENTADO: ThemeColorOption, ThemeColorPickerView, paleta visual de 21 cores, preview, suporte à cor personalizada legada e confirmação inline de exclusão.
PERSISTÊNCIA: Theme.ColorHex foi preservado sem alteração de tipo ou migração.
```
## Correção final — seleção de versículos e comparação

```text
STATUS: CONCLUÍDA
DECISÃO: a seleção de intervalo pertence ao estado de apresentação do BibliaTema; CollectionView não mantém seleção paralela.
IMPLEMENTADO: BibleVerseItemViewModel, VerseItems, SelectVerseCommand, RefreshVerseSelectionState, SelectionMode=None e DataTriggers por IsSelected.
NAVEGAÇÃO: CompareCommand envia coordenadas canônicas e metadados da referência por IAppNavigator para a rota interna única BibleComparison.
COMPARAÇÃO: parâmetros aplicados antes do carregamento; versões instaladas/habilitadas marcadas por padrão; comparação automática; cards sem listas aninhadas e com números dos versículos.
TESTES: toggle limpo, intervalo inverso, parâmetros de navegação, inicialização automática e rejeição de referência inválida.
VALIDAÇÃO: 19/19 testes aprovados; builds Android e Windows concluídos, com Windows em 0 avisos e 0 erros. A execução agregada dos quatro targets foi interrompida porque o target Apple não concluiu no ambiente Windows, após o Android ter sido gerado com sucesso.
```
# Checkpoint — módulo Mensagens funcional (2026-08-19)

- Os filtros de tipo e seletores horizontais usam seleção explícita controlada pelo ViewModel; tipo de filtro e tipo do editor permanecem propriedades distintas.
- O fluxo interno de referências usa `IMessageService`, `IMessageReferenceService`, `IMessageTopicService`, `ISavedReferenceService`, `IThemeService`, `IBibleRepository` e `IBibleVersionManager`; ViewModels não executam SQL.
- A identidade de uma referência salva é `(BookReferenceId, Chapter, VerseStart, VerseEnd)`. `GetOrCreateCanonicalAsync` reutiliza essa identidade e acrescenta temas por união, sem remover associações anteriores.
- `SavedReference.Comment` continua permanente; `MessageReference.Observation` é exclusivamente contextual à mensagem.
- Excluir um vínculo remove somente `MessageReference`. A referência salva e `ReferenceTheme` são preservados.
- `BibleReaderPage` não foi alterada.
# Checkpoint — Design System e modernização visual (2026-08-19)

- O projeto permanece em .NET 10; a menção a MAUI 8 na especificação foi tratada como contexto visual legado.
- A paleta global adota índigo, superfícies neutras, contraste alto e seleção em tint claro, com suporte Light/Dark.
- `Border` com `RoundRectangle 12` é o contêiner padrão de cards; estilos globais centralizam tipografia, texto bíblico, captions e estados de botões.
- O leitor preserva seus ViewModels, comandos, seleção por toque e eventos existentes; somente a composição visual foi modernizada.
- Flyout, Mensagens, Comparador e Configurações compartilham o mesmo vocabulário visual responsivo.
# Checkpoint — falha de inicialização Android corrigida (2026-08-19)

- Diagnóstico no aparelho `moto g86 5G`: `AppShell` falhava com `XamlParseException` porque `Surface` era solicitado como `StaticResource` durante a inicialização do Shell.
- O Shell passou a usar valores fundamentais locais para seu carregamento inicial, sem depender da ordem de resolução dos recursos globais.
- Cabeçalhos de seção desabilitados no flyout não podem ser o item inicial; `CurrentItem` agora é definido explicitamente como o primeiro item navegável (`Início`).
- APK Release foi instalado sem incremental deployment e validado por ADB: processo ativo, atividade em primeiro plano e ausência de exceção fatal no `logcat`.
# Checkpoint — Bíblia Tema Premium global (2026-08-19)

- A refatoração é estritamente visual: não altera commands, bindings, rotas, serviços, repositórios, banco ou regras de negócio.
- `Colors.xaml` é a fonte oficial da paleta `Bt*`; aliases anteriores apontam para a nova identidade para cobertura integral sem reescrever lógica.
- `Styles.xaml` centraliza controles, estados, espaçamentos, raios e Light/Dark.
- O flyout usa recursos locais mínimos apenas no carregamento inicial para evitar a regressão Android de resolução precoce de `StaticResource`; os valores são idênticos aos tokens globais.
- `BibleReaderPage` manteve layout funcional, seleção, navegação, comandos e carregamento; somente recursos visuais foram substituídos.

# Checkpoint — Adicionar referências (2026-08-20)

- `MessageBibleReferencesPage` é o único fluxo alterado; `BibleReaderPage`, `BibleComparisonPage` e `MainPage` permanecem intactas.
- A seleção dos versículos pertence ao ViewModel e usa uma única ação de toque. Filtros bíblicos podem usar seleção simples nativa, mas o template define explicitamente os estados premium.
- Cada operação escolhe um tema; ao persistir, o conjunto final é `temas existentes UNION tema selecionado`.
- Comentário é propriedade de `SavedReference`; observação e tópico são propriedades do vínculo `MessageReference`.
- Editar reutiliza o `ReferenceId` existente. Remover chama somente `IMessageReferenceService.DeleteAsync`, preservando a referência e `ReferenceTheme`.
- Validação automatizada: build completo Android/iOS/MacCatalyst/Windows em .NET 10 com 0 avisos e 0 erros; 23/23 testes aprovados, incluindo intervalo, tema único e fluxo persistente completo.
- Validação física desta execução ficou indisponível porque `adb devices` não listou aparelho conectado em 2026-08-20; nenhuma evidência manual foi presumida.

# Checkpoint — fechamento ao abrir Adicionar referências (2026-08-20)

- Diagnóstico no `moto g86 5G`: `InvalidOperationException: VisualStateGroup Names must be unique` ao materializar os chips de mensagem/versão/livro/capítulo.
- Causa: o template declarava `CommonStates` localmente enquanto o design system já fornecia o mesmo grupo ao `Border`.
- Correção: removidos os grupos locais duplicados; os estados permanecem centralizados no estilo global `Border`/`Bt*`.
- O primeiro aborto observado em APK Debug era independente (`Fast Deployment .__override__`); a validação definitiva foi feita com APK Release sem apagar os dados do usuário.
- Evidência no aparelho: formulário visível, processo ativo, atividade em primeiro plano, toque no seletor de versão executado e nenhum `FATAL EXCEPTION`, `XamlParseException` ou nova duplicidade de VisualState no log.

# Checkpoint — contraste de seleção e habilitação do vínculo (2026-08-20)

- O estado `Normal` implícito de `Border` tinha precedência sobre gatilhos de seleção. O estilo implícito passou a cuidar apenas da base visual; estados interativos pertencem aos componentes específicos.
- `BtSelectableChip` aplica na raiz do item: normal `#FFFFFF/#E0E0E0/#0A0A0A`; selecionado `#E4EDF7/#B8D4F0/#0066CC` com texto em negrito.
- Versão, livro e capítulo usam o componente; versículos usam o mesmo fundo/borda por `IsSelected`. A distinção foi confirmada visualmente no aparelho.
- O botão de adicionar estava corretamente desabilitado porque não havia mensagem salva/selecionada. O empty state agora oferece `Selecionar mensagem`, e o formulário informa dinamicamente todos os requisitos faltantes ou `Pronto para adicionar à mensagem.`.
- APK Release instalado no `moto g86 5G`; seleção de ACF, Gênesis, capítulo 1 e versículo 1 confirmada, processo ativo e log sem exceção fatal.

# Checkpoint — vínculo mensagem/referência e lista por tema (2026-08-20)

- A foto do aparelho demonstrou que o formulário estava sem mensagem: o botão parecia azul, mas `CanExecute` recusava a gravação para evitar `MessageReference` órfã.
- `MessagesViewModel.ReferencesCommand` agora navega com o `MessageId` persistido; `MessageBibleReferencesPage` recebe o parâmetro e seleciona a mesma mensagem antes de habilitar a inclusão.
- Sem mensagem, a tela mantém a ação `Selecionar mensagem` e informa o requisito faltante. Não é criado rascunho silencioso nem vínculo sem proprietário.
- Após incluir, o tema permanece selecionado, a referência canônica é criada/reutilizada, `ReferenceTheme` recebe união e `MessageReference` é gravado com comentário/observação separados.
- A lista inferior é filtrada por `SelectedTheme` e recebe o título `Referências vinculadas ao tema {nome}`, exibindo os intervalos já associados naquele contexto de mensagem.
- Validação: Windows 0 avisos/0 erros, 23/23 testes, APK Release instalado e processo Android em primeiro plano sem exceção fatal.

# Checkpoint — fluxo de Tópicos (2026-08-20)

- Causa dos botões desabilitados: a tela não possuía mensagem selecionada; adicionalmente, carregamentos internos eram executados dentro do mesmo bloqueio `IsBusy` e retornavam sem atualizar a lista.
- `MessageTopicsViewModel` passou a usar `IMessageService` e `IMessageTopicService`, sem acesso direto de ViewModel ao repositório.
- O fluxo exige mensagem persistida e título não vazio. Novo, Salvar, Excluir, Mover acima e Mover abaixo possuem `CanExecute` específico e recebem atualização em toda mudança relevante.
- A tela Mensagens envia `MessageId` ao abrir Tópicos; acesso direto oferece `Selecionar mensagem`. Nenhum tópico órfão é criado.
- Carregamento, criação, atualização, exclusão e reordenação usam operações internas que não disputam o mesmo `IsBusy`; a lista é recarregada e mantém o tópico salvo/selecionado.
- Visual premium: header `#0D1E30`, superfície `#FFFFFF`, borda `#E0E0E0`; tópico selecionado usa `#E4EDF7`, `#B8D4F0` e texto `#0066CC`.
- Validação: teste de integração cobre criar, atualizar, reordenar e excluir; 24/24 testes aprovados, Windows 0 avisos/0 erros e APK Release validado no aparelho sem exceção fatal.

# Checkpoint — fluxo de Mensagens (2026-08-20)

- Causa dos botões permanentemente desabilitados: o filtro inicial disparava carregamento assíncrono antes da construção dos comandos; a notificação acessava comandos ainda não inicializados e podia manter `IsBusy` ativo.
- A inicialização agora define o filtro sem disparar carregamento prematuro. Novo, Buscar, Salvar, Excluir, Tópicos e Referências atualizam seus estados conforme ocupação, título e existência de mensagem persistida.
- Os tipos do domínio permanecem inalterados, mas a apresentação usa opções explícitas em português: Mensagem, Pregação, Estudo e Devocional.
- O editor exige título para salvar. Tópicos e Referências só habilitam após a mensagem ser persistida, impedindo relacionamentos órfãos.
- A tela adota o tema premium solicitado: header `#0D1E30`, fundo `#F4F6F9`, superfície `#FFFFFF`, primária `#0066CC`; selecionado `#E4EDF7/#B8D4F0/#0066CC` e normal `#FFFFFF/#E0E0E0/#0A0A0A`.
- Validação: teste de regressão cobre estado inicial, habilitação de Salvar e mapeamento dos tipos em português; 25/25 testes aprovados, Windows 0 avisos/0 erros e APK Release instalado no `moto g86 5G`, com Novo e Buscar habilitados e sem exceção fatal.

# Checkpoint — organização da tela Versões (2026-08-20)

- O `FlexLayout` de ações foi substituído por uma grade estável de duas colunas dentro de card próprio, eliminando cortes, larguras irregulares e sobreposição no Android.
- Adicionar e Ativar/desativar usam a ação primária `#0066CC`; Validar usa superfície normal; Definir como padrão usa `#D4AF37`; Remover importada usa o estilo destrutivo e preserva sua regra de habilitação somente para versões não embarcadas.
- A lista ocupa o espaço disponível sem altura rígida. Cards normais usam `#FFFFFF/#E0E0E0/#0A0A0A`; o selecionado usa `#E4EDF7/#B8D4F0/#0066CC`.
- Comandos, importação, validação, ativação, definição de padrão e remoção não tiveram suas regras de negócio alteradas.

# Checkpoint — Minhas Referências e Temas (2026-08-20)

- Minhas Referências usa o seletor premium compartilhado para versão, livro, capítulo, versículo inicial e versículo final. Normal: `#FFFFFF/#E0E0E0/#0A0A0A`; selecionado: `#E4EDF7/#B8D4F0/#0066CC`.
- Os dois intervalos de versículo receberam rótulos inequívocos e a referência formatada, prévia bíblica, comentário, temas e ações foram agrupados em cards com hierarquia visual consistente.
- Temas usa cards selecionáveis compactos, separados por 4 px, com a mesma semântica visual global. O seletor de cores ganhou contorno e escala explícitos para a cor escolhida.
- A prévia do tema usa uma faixa colorida sobre superfície neutra, mantendo legibilidade para cores claras e escuras; `SelectedColorName` agora é notificado imediatamente ao trocar a cor.
- Cancelar, Salvar e Excluir foram separados por função visual; a confirmação destrutiva preserva as referências bíblicas e mantém a regra de negócio existente.
- As duas telas são responsivas: empilhadas no celular e em duas colunas em janelas largas. Nenhum ViewModel recebeu SQL ou acesso direto a SQLite.

# Checkpoint — Home e Versículo do Dia (2026-08-20)

- `IDailyVerseService` seleciona uma referência canônica diária entre cinco categorias e persiste data, livro, capítulo, versículo e categoria em `ISettingsService`.
- O texto nunca é persistido nem hardcoded: é relido por `IBibleRepository` na versão ativa (ou primeira instalada/habilitada), preservando a referência do dia quando a tradução muda.
- A Home prioriza o card devocional, mantém atalhos funcionais em duas colunas no mobile e conserva os contadores úteis em “Sua biblioteca”.
- “Ler capítulo inteiro” usa a rota existente `BibleReader`; parâmetros opcionais foram adicionados de forma compatível para abrir livro/capítulo e selecionar o versículo sem alterar o uso normal do leitor.
