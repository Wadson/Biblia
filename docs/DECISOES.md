# Registro de decisões e checkpoints

## 2026-08-20 — Compartilhar uma Palavra

- `BibleReaderPage`, regras de referências e `DailyVerseService` não foram alterados.
- O render final usa SkiaSharp 4.151.1, compatível com .NET 10/MAUI, e não screenshot da interface.
- Pexels fica atrás de `INatureMediaService`; nenhuma chave é versionada e o proxy WR Soft é o caminho recomendado para produção.
- O Studio usa seletores temáticos próprios e catálogo curado do CDN Pexels quando não há proxy/chave, preservando gradientes offline e mensagens explícitas para falha de conexão.
- As prévias remotas do Studio são baixadas e validadas no cache antes de entrarem na galeria; a UI não depende mais do download silencioso do controle `Image`.


## 2026-08-20 — Atalho Home em Relatórios

- A tela raiz `ReportsPage` mantém o menu do Flyout e também oferece um botão grande `⌂ Home` no canto superior direito do cabeçalho do formulário.
- O atalho navega diretamente para a rota raiz `//Home`, sem criar uma nova página na pilha.


## 2026-08-20 — Espaçamento entre referência e texto bíblico

- O espaço posterior da linha de referência foi reduzido de 4 pt para 1 pt.
- O texto bíblico passou a declarar espaço anterior de 0 pt; nenhum outro elemento do PDF foi alterado.


## 2026-08-20 — Compactação das referências no PDF

- Mantido integralmente o layout editorial aprovado.
- O título da referência passou de 11 pt para 9,5 pt e a versão bíblica foi movida para a mesma linha em 7,5 pt, reduzindo o espaço vertical de cada bloco.


## 2026-08-20 — Relatórios como gerador profissional

- `BookReferenceId` deixou de ser texto de apresentação; `ReportService` resolve `BookName` no catálogo real e entrega `FormattedReference` ao PDF/UI.
- Foi adotado `PDFsharp-MigraDoc` 6.2.4 (MIT), pois a versão Core suporta .NET 10 e MAUI, enquanto QuestPDF não oferece suporte atual a MAUI.
- Open Sans regular/semibold foi incorporada como recurso para garantir Unicode de forma consistente em Android e Windows.
- Referências por temas usam consulta parametrizada explícita em `ISavedReferenceRepository.GetByThemeIdsAsync`; referências presentes em vários temas são deduplicadas e preservam os badges dos temas relacionados.


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

# Checkpoint — fluxo único Mensagens e Pregações (2026-08-20)

- O Flyout expõe somente `Mensagens e Pregações` para o domínio; Mensagens, Tópicos, Referências nas mensagens e Duplicação permanecem como rotas internas para compatibilidade.
- `MessageBibleReferencesPage` é a experiência consolidada: escolha/criação da mensagem, tema obrigatório, seletor bíblico, comentário, referências vinculadas, ordenação, duplicação e acesso a relatório/PDF.
- Tema, versão, livro e capítulo permanecem após cada inclusão; somente seleção bíblica e comentário contextual são limpos para agilizar séries do mesmo tema.
- `MessageTopic` continua opcional como “Seção (modo avançado)” e `TopicId = null` permanece suportado. Observação foi reclassificada visualmente como nota contextual opcional.
- A ordenação usa `IMessageReferenceService.MoveAsync` e a implementação transacional existente de `ReorderReferencesAsync`; nenhum schema ou dado antigo foi removido.
- O formulário simples não exibe seção/tópico nem nota contextual. Na primeira inclusão sem mensagem selecionada, cria automaticamente uma Pregação com título derivado do tema e grava o vínculo na mesma operação; referências novas usam `TopicId = null` e observação nula.

# Checkpoint — Configurações, Backup e Sobre (2026-08-20)

- Os marcadores visuais `MENSAGENS E PREGAÇÕES` e `SISTEMA` foram removidos do Flyout; as entradas navegáveis permanecem.
- `SettingsViewModel` coordena `IBackupService`, `IBibleVersionManager`, exportação/compartilhamento de arquivo, seleção de backup, informações reais do app e navegação, sem lógica específica de Android.
- Criar backup reutiliza integralmente `BackupService`; exportar e compartilhar enviam o ZIP real ao share sheet do sistema, permitindo Arquivos, Drive, WhatsApp, Telegram e demais destinos instalados.
- Restauração seleciona ZIP, executa `ValidateAsync`, exige confirmação customizada e chama `RestoreAsync`, preservando o backup de segurança existente.
- Settings lista apenas versões instaladas, destaca a ativa e delega gerenciamento completo à rota `BibleVersions`. Sobre usa versão/build reais via `AppInfo`.
# Checkpoint — backup com destino e Relatórios premium (20/08/2026)

- “Salvar uma cópia” passou a criar um backup atualizado e abrir o salvador de arquivos nativo, permitindo ao usuário escolher pasta e nome no gerenciador do sistema.
- A ação redundante “Criar backup” foi removida; salvar e compartilhar geram o ZIP automaticamente.
- “Compartilhar” preserva a folha nativa de aplicativos e “Restaurar” preserva validação e confirmação.
- Relatórios recebeu cartões de indicadores, seletor de mensagens com estados normal/selecionado, ações temáticas e estado vazio explícito.
- O projeto permanece em .NET 10; Microsoft.Maui.Controls foi atualizado para 10.0.60, requisito do CommunityToolkit.Maui 14.2.2 usado pelo salvador nativo.
# Checkpoint — navegação Voltar e saída global (20/08/2026)

- Todas as `ContentPage` foram classificadas entre ROOT e SECONDARY em `docs/TELAS.md`.
- O mecanismo nativo do Shell foi preservado para que botão visual e Android Back compartilhem a mesma pilha.
- O Flyout recebeu a ação Sair no rodapé, com confirmação customizada e serviço de plataforma abstraído por `IApplicationExitService`.
- A saída não é exibida no iOS e não utiliza `Environment.Exit`, `Process.Kill` ou `DisplayAlert`.
# Checkpoint — Voltar explícito nos formulários secundários (20/08/2026)

- Este checkpoint foi substituído pela padronização global descrita ao final do documento.
# Checkpoint — experiência imersiva “Ler Bíblia” (20/08/2026)

- O painel alto com fileiras permanentes foi substituído por uma barra compacta única: Livro, Capítulo, Versão e Mais.
- A leitura permanece virtualizada em `CollectionView`, agora com texto contínuo, cabeçalho de capítulo discreto e seleção no padrão `BtSelectedBackground`/`BtSelectedBorder`.
- A regra de seleção foi preservada: primeiro toque inicia, segundo define intervalo e novo toque no único versículo desmarca.
- Copiar e Comparar permanecem nos Commands existentes e aparecem em barra contextual somente durante uma seleção.
- Tipografia persistente foi movida para Opções de leitura; anterior/próximo ficam no final do capítulo.
- A barra bíblica recolhe ao rolar para baixo e reaparece ao rolar para cima, por comportamento estritamente visual da View.
## 2026-08-20 — Seletores temáticos no leitor bíblico

- Os seletores nativos de livro, capítulo e versão da tela `Ler Bíblia` foram substituídos por painéis internos do design system.
- A alteração preserva o fluxo e os métodos existentes do `BibleReaderViewModel`; somente a apresentação e a captura da escolha foram modernizadas.
- As opções normais usam superfície branca e borda `BtBorder`; a opção atual usa `BtSelectedBackground`, `BtSelectedBorder` e `BtPrimary`.

# Checkpoint — miniaturas remotas no Estúdio de cards (20/08/2026)

- A busca baixa de seis a oito miniaturas JPEG válidas para o cache privado do aplicativo antes de publicá-las na galeria.
- A galeria é renderizada por HTML local em `WebView`, usando `data:` URLs produzidas somente a partir dos arquivos validados. Isso evita a falha do handler de imagens do Android ao abrir caminhos do cache privado e não expõe conteúdo local à rede.
- Os toques nas miniaturas usam o esquema interno `bibliatema://photo/{id}`, interceptado pela página e encaminhado ao ViewModel; a opção selecionada mantém a borda azul do tema.
- Falha de download, arquivo incompleto, timeout ou ausência de conexão não produz cartões vazios: o usuário recebe a mensagem de conexão e continua com os fundos locais.
- A pesquisa passou a ser paginada em lotes de oito. “Carregar mais fotos” acumula novos resultados, preserva o lote anterior e elimina IDs repetidos.
- Galeria e prévia recebem uma nova fonte HTML local a cada seleção; isso força o handler Android a redesenhar imediatamente o JPEG escolhido, sem aguardar a geração final do PNG.
- O grid mantém altura controlada e rolagem vertical interna, com barra permanente em azul primário sobre trilho `Selected Background`; os novos lotes podem ser percorridos sem deslocar o formulário inteiro.
- Como o WebView Android oculta o scrollbar CSS/nativo em alguns aparelhos, o indicador passou a ser desenhado dentro do HTML: trilho de 13 px e cursor azul com mínimo de 46 px, sincronizado à posição real da rolagem.

# Checkpoint — cabeçalho Voltar global e reutilizável (20/08/2026)

- A auditoria de `Presentation/Views` e das rotas registradas no Shell confirmou seis páginas secundárias: Comparar versões, Mensagens, Tópicos, Referências da mensagem, Duplicar mensagem e Criar card.
- Todas usam o mesmo `PageHeaderView`: `BtHeader` (`#0D1E30`), título responsivo, ação `← Voltar` em `BtGoldSoft`, área mínima de 44dp, semântica acessível e estados hover/pressionado.
- O Back nativo do Shell foi ocultado somente como elemento visual nessas páginas. O componente chama `IAppNavigator.GoBackAsync()` e o gesto/botão Android continua usando a mesma pilha, preservando contexto e estado da página anterior.
- A proteção interna impede duas navegações simultâneas em toques rápidos. Toolbars textuais duplicadas foram removidas.
- Páginas raiz continuam sem Voltar e mantêm o menu Flyout. A regra para páginas futuras foi registrada em `docs/DESIGN_SYSTEM.md`.

# Checkpoint — retorno também nas entradas do Flyout e barra de galeria tátil (20/08/2026)

- Por requisito de uso, Pesquisar na Bíblia, Versões da Bíblia, Temas, Minhas Referências, Mensagens e Pregações, Pesquisa global, Relatórios e Configurações também receberam o `PageHeaderView`.
- Nessas entradas, o componente retorna pela pilha quando ela existe e usa `//Home` como destino seguro quando a página é a raiz do item do Flyout. O bloqueio de navegação concorrente permanece centralizado.
- A barra vertical do Estúdio de cards passou de 13 para 28 px, com cursor mínimo de 56 px e eventos de ponteiro para toque, arraste e toque no trilho.
- A prévia de Tema centraliza, como um único conjunto visual, o indicador de cor e o nome do tema.

# Checkpoint — correção do Voltar e controles laterais da galeria (20/08/2026)

- O `InternalBackCommand` do `PageHeaderView` passou a ser criado antes de `InitializeComponent`; assim, o binding XAML recebe o comando já na primeira avaliação e o botão funciona em todas as instâncias do cabeçalho.
- A barra arrastável da galeria foi substituída por setas laterais temáticas de 44×54 px. Toque executa avanço por página e toque prolongado repete a rolagem; os controles indicam visualmente quando o início ou o fim foi alcançado.

# Checkpoint — integridade de Temas e resiliência do Estúdio de cards (22/08/2026)

- O estado de edição de Tema passou a ser explícito em `EditingThemeId`; selecionar um item não altera o modo do formulário, e editar exige a ação dedicada. Novo, cancelar, inclusão concluída, exclusão e recarga zeram o identificador.
- A persistência mantém `INSERT` e `UPDATE ... WHERE Id=$id` separados. O update continua exigindo exatamente uma linha, e a unicidade case-insensitive do nome agora é traduzida para uma validação amigável sem alterar o registro existente.
- Comandos assíncronos de UI passaram a conter e registrar exceções no limite de `async void`, evitando exceções não observadas no dispatcher WinUI.
- O renderizador valida o bitmap antes de acessar suas dimensões, usa gradiente offline para imagem inválida, resolve fontes pelo assembly do próprio serviço e usa a fonte padrão como contingência.
- Downloads de imagens agora validam JPEG, escrevem primeiro em arquivo temporário, substituem o cache atomicamente e removem arquivos parciais; previews expõem caminhos locais diretamente, sem conversão ambígua de path Windows para URI.
- Validação: 42 testes aprovados; builds Windows `net10.0-windows10.0.19041.0` e Android `net10.0-android/android-arm64` concluídos sem erros.

# Checkpoint — backup e restauração multiplataforma (22/08/2026)

- `BackupService` permanece independente da UI e das plataformas. A criação passou a usar a API de snapshot do SQLite, incluindo dados ainda presentes no WAL, antes de montar o ZIP com `manifest.json` e `bibliatema.db`.
- O banco extraído é validado por `PRAGMA integrity_check` e pela versão real de `SchemaMigration` antes de qualquer substituição.
- `AppDatabase.ReplaceAsync` coordena limpeza dos pools, troca atômica com arquivo de rollback, reset de `_initialized` e reinicialização. Se a abertura do banco restaurado falhar, o banco anterior é recolocado e reinicializado.
- O salvamento distingue resultado salvo de cancelamento. WinUI e Android usam seus seletores nativos pelo `FileSaver`; iOS e MacCatalyst usam o fallback nativo suportado pelo mesmo Toolkit.
- No Android, resultados do `FilePicker` sem path físico são copiados do stream do `content URI` para cache privado antes da validação e restauração.
- Validação: 48 testes aprovados; builds Windows `net10.0-windows10.0.19041.0` e Android `net10.0-android/android-arm64` concluídos sem erros.

# Checkpoint — pesquisa bíblica e responsividade em Mensagens e Pregações (26/08/2026)

- A tela de referências de Mensagens e Pregações recebeu pesquisa por palavra ou frase no texto bíblico. A consulta usa exclusivamente o código da versão selecionada e mantém os bancos de `Versoes` somente leitura.
- Os resultados reutilizam a coleção e a lista visual `Verses`; não existe uma segunda lista de resultados. Cada item identifica livro, capítulo e versículo e, ao ser tocado, define a localização canônica usada pelo mesmo comando de inclusão da referência no tema selecionado.
- Os rótulos dos tipos de mensagem passaram a usar português. O reposicionamento horizontal de versão, livro e capítulo usa `MakeVisible`, evitando o recorte dos itens nas bordas.
- A lista inferior de referências passou a preencher a largura disponível, quebrar textos longos e distribuir suas ações em `FlexLayout` com quebra de linha, impedindo que o botão Remover seja cortado no computador ou celular.
- Na pesquisa bíblica, referências que já possuem ao menos um tema são identificadas por coordenada canônica e intervalo, recebem o selo “Já adicionada” e não podem ser escolhidas novamente. Uma inclusão bem-sucedida atualiza o selo imediatamente na lista atual.
- O layout adaptativo existente foi preservado: duas colunas a partir de 800 dp e fluxo vertical em telas estreitas.
- A tela Relatórios substituiu o `FlexLayout` externo por um grid adaptativo com colunas efetivamente limitadas. Isso força textos, cartões e a prévia a respeitarem a largura disponível no computador e mantém o empilhamento em telas estreitas.
- Validação: 11 testes relacionados a mensagens aprovados; build Windows isolado concluído com 0 avisos e 0 erros. Na suíte completa anterior, 51/52 testes passaram; a falha repetível e fora deste escopo está em `BackupServiceTests.Restore_ReplacesDataAndDatabaseRemainsOperational`, por colisão do nome de dois arquivos de backup criados no mesmo segundo.

# Checkpoint — organização do menu lateral no Windows (26/08/2026)

- O Flyout passou a usar largura de 320 dp e cartões centralizados de 296 dp para impedir que a medição intrínseca dos títulos produza botões com larguras diferentes ou cortados.
- Cabeçalhos de seção usam apresentação compacta, sem ícone, borda ou superfície de botão. Foram incluídas as divisões ausentes “Mensagens e Pregações” e “Sistema e Configurações”.
- O cabeçalho da marca foi reduzido para 152 dp, ampliando a área rolável; o rodapé “Sair” permanece fixo e adota a mesma largura dos itens de navegação.
- A ordem funcional das rotas foi preservada e a lista central continua com rolagem nativa no computador e no celular.
- Validação: build Windows isolado concluído com 0 avisos e 0 erros.

# Checkpoint — auditoria responsiva do Gerador de pregações (26/08/2026)

- O conteúdo do `ScrollView` e o grid principal de Relatórios agora preenchem a largura disponível até o limite de 1180 dp, removendo a faixa vazia que comprimia e cortava a prévia no Windows.
- A ação “Gerar PDF” usa uma coluna limitada de 132 dp, com título quebrável na coluna restante; o painel e o cartão de prévia também declaram preenchimento horizontal explícito.
- Títulos, subtítulos, identificação da versão, introdução e demais textos editoriais passam a quebrar dentro da largura do cartão, sem criar medida horizontal infinita.
- O seletor de origem empilha suas opções em janelas estreitas e usa duas colunas a partir de 900 dp, preservando integralmente os rótulos.
- Após “Gerar prévia”, a página retorna ao topo para apresentar imediatamente “Gerar PDF”, sem exigir que o usuário procure a ação fora da área visível.
- Estados visuais, regras de geração, conteúdo do relatório e exportação permaneceram inalterados.
- Validação: build Windows isolado concluído com 0 avisos e 0 erros.

# Checkpoint — layout desktop de Mensagens e Pregações (27/08/2026)

- Em janelas a partir de 800 dp, a tela mantém o seletor bíblico na coluna esquerda e reúne, na coluna direita, o formulário de vínculo e as referências já adicionadas.
- A lista de referências deixa de ocupar uma faixa integral abaixo do seletor, aproveitando melhor a área visível em computadores com resolução de 1024×768 ou superior e reduzindo a rolagem necessária.
- Em telas estreitas, o painel direito continua empilhado depois do seletor bíblico, preservando o comportamento responsivo anterior.
- Os controles, bindings, comandos e o fluxo de seleção de versão, livro, capítulo e versículo não foram alterados; a mudança é exclusivamente de composição visual.
- Validação: 11 testes relacionados a mensagens aprovados; build e publicação Windows Release executados para `D:\Publicacao\Biblia`.

# Checkpoint — fluxo centrado em Tema (27/08/2026)

- O fluxo principal foi reduzido a `Tema → versículos`: o usuário escolhe um tema e vincula o trecho bíblico diretamente à referência salva, sem criar ou selecionar Mensagem, Pregação, Estudo ou Devocional.
- A tela consolidada anteriormente chamada “Mensagens e Pregações” foi preservada e passou a se chamar “Vinculação de Temas” no menu e no cabeçalho; as rotas antigas de edição de mensagens, tópicos, referências contextuais e duplicação deixaram de ser registradas no Shell.
- A pesquisa global não consulta mais mensagens, tópicos ou observações contextuais e a Home apresenta somente Temas e Versículos vinculados.
- Relatórios continuam com prévia, subtítulo, introdução, conclusão, versão bíblica e exportação PDF, mas aceitam um único Tema como origem. O nome do Tema é também o título automático do documento e o tipo editorial exibido é “TEMA”.
- O banco existente e seus dados legados foram preservados para compatibilidade; nenhuma migração destrutiva foi aplicada e os bancos bíblicos permanecem somente leitura.

# Checkpoint — formulário de Temas responsivo no computador (27/08/2026)

- A largura útil da tela de Temas passou a preencher o computador até 1180 dp, mantendo margens consistentes.
- A partir de 900 dp, a lista de temas e o editor são organizados lado a lado, com proporção equilibrada e espaçamento explícito; no celular, continuam empilhados.
- O cabeçalho ganhou composição adaptativa: título e ações ficam em uma linha no computador e as ações ocupam uma segunda linha dividida no celular, evitando cortes em 1024×768.
- Pesquisa, seleção, criação, edição, cores, validação e exclusão mantêm os mesmos comandos e regras de persistência.

# Checkpoint — publicação MSIX pelo Visual Studio (27/08/2026)

- A configuração global `WindowsPackageType=None`, que forçava somente a distribuição Windows não empacotada, foi removida.
- O target Windows volta a usar o empacotamento MSIX padrão do .NET MAUI, permitindo que o Visual Studio apresente “Publicar” ao clicar no projeto `Biblia` com `Windows Machine` selecionado.
- O manifesto Windows existente foi preservado; certificado, versão e destino do instalador são definidos pelo assistente de publicação do Visual Studio.
