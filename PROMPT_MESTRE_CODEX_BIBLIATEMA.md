# PROMPT MESTRE PARA O CODEX — DESENVOLVIMENTO COMPLETO DO BIBLIATEMA

> **Projeto:** BibliaTema  
> **Tecnologia:** .NET MAUI / .NET 10  
> **Plataformas-alvo:** Windows, Android, iOS e macOS (MacCatalyst)  
> **Persistência:** SQLite  
> **Arquitetura:** Offline-first, multi-versão, banco bíblico separado do banco do usuário  
> **Situação inicial:** O projeto .NET MAUI já foi criado no Visual Studio. O desenvolvimento funcional deverá partir do zero dentro desse projeto.  
> **Pasta local das versões bíblicas existentes:** `D:\Projetos\Biblia\Versoes`

---

# 0. PAPEL DO CODEX

Você será o **agente principal de engenharia de software** responsável por implementar integralmente o aplicativo **BibliaTema**.

Sua responsabilidade não é produzir apenas exemplos, protótipos, pseudocódigo ou trechos isolados.

Você deverá:

1. analisar o projeto existente;
2. planejar;
3. implementar;
4. compilar;
5. testar;
6. corrigir;
7. validar;
8. documentar;
9. somente então avançar para a próxima etapa.

O resultado esperado é um **aplicativo funcional**, organizado, testável, multiplataforma, com persistência real, navegação real, telas reais, buscas reais, relatórios reais e suporte real a múltiplas versões bíblicas SQLite.

---

# 1. REGRA ABSOLUTA: NÃO PULAR ETAPAS

## 1.1. Regra principal

**NÃO PULE NENHUMA ETAPA DESTE DOCUMENTO.**

Você deverá executar as fases na ordem estabelecida.

Não avance para a próxima fase se existir qualquer um dos seguintes problemas na fase atual:

- erro de compilação;
- referência quebrada;
- migration inválida;
- serviço não registrado;
- tela não navegável;
- comando não funcional;
- código temporário;
- TODO essencial;
- mock usado no lugar de implementação real;
- tratamento de erro ausente em operação crítica;
- teste obrigatório falhando;
- banco corrompido;
- dependência quebrada;
- regra de negócio violada.

Quando encontrar um problema:

1. identifique;
2. corrija;
3. recompile;
4. execute os testes relacionados;
5. só depois continue.

---

# 2. REGRAS DE EXECUÇÃO DO CODEX

Antes de alterar qualquer arquivo:

1. examine toda a solução;
2. localize o `.sln`;
3. localize o `.csproj` MAUI;
4. examine `MauiProgram.cs`;
5. examine `App.xaml`;
6. examine `AppShell.xaml`, caso exista;
7. examine `Resources`;
8. examine os targets configurados;
9. procure arquivos `AGENTS.md` ou equivalentes no repositório;
10. respeite instruções existentes no repositório que não contrariem este prompt.

Crie e mantenha durante o desenvolvimento:

```text
docs/
    ARQUITETURA.md
    MODELO_DADOS.md
    BANCOS_BIBLICOS.md
    REGRAS_NEGOCIO.md
    TELAS.md
    RELATORIOS.md
    TESTES.md
    DECISOES.md
```

Também crie na raiz:

```text
AGENTS.md
```

Esse arquivo deve resumir:

- arquitetura;
- comandos de build;
- comandos de teste;
- convenções;
- restrições;
- regra de não acessar SQLite diretamente nas Views;
- regra de não correlacionar versões pelo `verse.id`;
- regra de validação antes de concluir tarefas.

---

# 3. VISÃO DO PRODUTO

O **BibliaTema** será uma plataforma pessoal de conhecimento bíblico.

Ele deverá permitir:

- leitura bíblica offline;
- várias traduções da Bíblia;
- troca de versão durante a leitura;
- comparação entre traduções;
- pesquisa bíblica;
- seleção de livro, capítulo e versículo;
- seleção de intervalos;
- cadastro de referências bíblicas;
- associação de uma referência a vários temas;
- comentários permanentes;
- criação de mensagens;
- criação de pregações;
- criação de estudos;
- criação de devocionais;
- criação de tópicos;
- reutilização de referências em várias mensagens;
- observações específicas por mensagem;
- pesquisa avançada;
- relatórios;
- geração de PDF;
- backup;
- restauração;
- funcionamento offline;
- interface responsiva;
- tema claro e escuro.

---

# 4. RESTRIÇÕES TECNOLÓGICAS

Use:

- .NET 10;
- .NET MAUI;
- C#;
- XAML;
- SQLite;
- MVVM;
- Dependency Injection;
- programação assíncrona onde apropriado;
- SQL parametrizado;
- camadas bem separadas.

## 4.1. Não usar UI nativa diretamente

**Não criar telas específicas usando WinUI diretamente, Android XML nativo, UIKit, AppKit ou qualquer interface paralela específica de plataforma.**

As telas de negócio devem ser feitas em:

- .NET MAUI;
- XAML;
- controles MAUI;
- componentes próprios reutilizáveis;
- estilos próprios.

Código específico de plataforma somente poderá existir quando for realmente necessário para integração de sistema operacional, por exemplo:

- seletor de arquivo;
- compartilhamento;
- permissões;
- impressão;
- caminhos locais;
- armazenamento seguro;
- integração de sistema.

Mesmo nesses casos:

- esconder a implementação atrás de uma interface de serviço;
- não colocar regra de negócio em código específico de plataforma.

---

# 5. ARQUITETURA OBRIGATÓRIA

Organize o projeto de forma compatível com:

```text
BibliaTema/
│
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── Rules/
│   └── Exceptions/
│
├── Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   ├── UseCases/
│   └── Validators/
│
├── Infrastructure/
│   ├── AppDatabase/
│   ├── BibleDatabases/
│   ├── Repositories/
│   ├── Backup/
│   ├── Reports/
│   ├── Files/
│   └── Logging/
│
├── Presentation/
│   ├── Views/
│   ├── ViewModels/
│   ├── Components/
│   ├── Behaviors/
│   ├── Converters/
│   └── Navigation/
│
├── Resources/
│   ├── Styles/
│   ├── Fonts/
│   ├── Images/
│   └── Raw/
│
└── Tests/
    ├── Unit/
    ├── Integration/
    ├── BibleDataValidation/
    └── Application/
```

Adapte à solução existente quando necessário, mas preserve a separação de responsabilidades.

---

# 6. REGRA FUNDAMENTAL DE DADOS

Existem **dois tipos completamente diferentes de banco**.

## 6.1. Bancos bíblicos

Cada versão da Bíblia possui um arquivo SQLite próprio.

Exemplos:

```text
ACF.sqlite
ARA.sqlite
ARC.sqlite
AS21.sqlite
KJF.sqlite
NAA.sqlite
NBV.sqlite
NTLH.sqlite
NVI.sqlite
NVT.sqlite
...
```

Esses bancos devem ser tratados principalmente como **somente leitura**.

Eles representam fontes bíblicas.

## 6.2. Banco do aplicativo

Crie um banco separado para dados produzidos pelo usuário.

Ele armazenará:

- temas;
- referências salvas;
- comentários;
- mensagens;
- tópicos;
- vínculos;
- observações;
- favoritos;
- configurações;
- estado da aplicação;
- catálogo das versões;
- histórico;
- dados de backup;
- futuras extensões.

**Nunca misture o conteúdo do usuário dentro dos bancos bíblicos.**

---

# 7. ESTRUTURA DOS BANCOS BÍBLICOS JÁ ANALISADOS

As versões atualmente analisadas utilizam:

```sql
book
metadata
verse
```

Estrutura principal:

```sql
CREATE TABLE book (
    id INTEGER PRIMARY KEY,
    book_reference_id INTEGER,
    testament_reference_id INTEGER,
    name VARCHAR(50)
);
```

```sql
CREATE TABLE metadata (
    key VARCHAR(255) PRIMARY KEY,
    value VARCHAR(255)
);
```

```sql
CREATE TABLE verse (
    id INTEGER PRIMARY KEY,
    book_id INTEGER,
    chapter INTEGER,
    verse INTEGER,
    text TEXT,
    FOREIGN KEY (book_id) REFERENCES book (id)
);
```

Apesar do esquema comum:

**NÃO ASSUMA QUE `verse.id` REPRESENTA O MESMO VERSÍCULO EM TODAS AS TRADUÇÕES.**

A identidade lógica será:

```text
BookId + Chapter + Verse
```

Para trechos:

```text
BookId + Chapter + VerseStart + VerseEnd
```

---

# 8. REFERÊNCIA BÍBLICA CANÔNICA

Crie um Value Object semelhante a:

```text
CanonicalBibleReference
```

Campos:

```text
BookId
Chapter
VerseStart
VerseEnd?
```

Regras:

- `BookId > 0`;
- `Chapter > 0`;
- `VerseStart > 0`;
- `VerseEnd`, se informado, deve ser `>= VerseStart`.

Formatação centralizada:

```text
João 3:16
João 3:16-18
1 Coríntios 4:1-6
```

Não duplique lógica de formatação em ViewModels ou Views.

---

# 9. VERSIFICAÇÃO E DIVERGÊNCIAS

Os bancos podem possuir diferenças de:

- quantidade de registros;
- versículos ausentes;
- coordenadas duplicadas;
- agregação de versículos;
- numeração;
- conteúdo.

Portanto:

- nunca correlacione traduções por ID sequencial;
- use referência canônica;
- trate ausência;
- trate duplicidade;
- trate ambiguidade;
- não escolha silenciosamente um resultado duplicado;
- registre anomalias;
- apresente mensagem apropriada na comparação.

Crie:

```text
BibleValidationService
```

com verificações de:

- `PRAGMA integrity_check`;
- tabelas obrigatórias;
- colunas obrigatórias;
- metadata;
- livros;
- capítulos;
- referências duplicadas;
- coordenadas inválidas;
- referências órfãs.

---

# 10. CATÁLOGO DE VERSÕES

Crie entidade/configuração:

```text
BibleVersion
```

Com campos similares a:

```text
Id
Code
DisplayName
Language
DatabaseFileName
InstalledPath
SchemaVersion
LicenseName
LicenseText
Attribution
IsInstalled
IsEnabled
IsBundled
InstalledAt
UpdatedAt
ValidationStatus
ValidationMessage
```

Exemplos de `Code`:

```text
ACF
ARA
ARC
AS21
KJF
NAA
NBV
NTLH
NVI
NVT
```

Crie:

```text
BibleVersionManager
```

Responsável por:

- listar versões;
- identificar instaladas;
- ativar/desativar;
- selecionar versão padrão;
- resolver caminho do banco;
- importar nova versão;
- validar versão;
- impedir colisão;
- armazenar metadados.

---

# 11. INSTALAÇÃO INICIAL DAS VERSÕES

Há versões bíblicas disponíveis no computador de desenvolvimento em:

```text
D:\Projetos\Biblia\Versoes
```

## 11.1. Durante o desenvolvimento

Inspecione essa pasta.

Enumere todos os:

```text
*.sqlite
*.db
```

que sejam versões bíblicas válidas.

Valide todos.

## 11.2. Distribuição do aplicativo

O instalador/pacote do aplicativo deverá incluir todas as versões aprovadas para distribuição.

Não crie dependência em:

```text
D:\Projetos\Biblia\Versoes
```

em tempo de execução.

Essa pasta é **fonte de desenvolvimento/build**, não caminho definitivo do usuário.

As versões aprovadas deverão ser incorporadas ao pacote como recursos apropriados ou copiadas pelo processo de build/publicação.

## 11.3. Primeira execução

Na primeira execução:

1. verificar diretório privado de dados do aplicativo;
2. verificar manifesto de versões;
3. instalar/copiar os bancos bíblicos empacotados;
4. não sobrescrever versão igual sem necessidade;
5. validar cada banco;
6. registrar no catálogo local;
7. criar banco do aplicativo;
8. executar migrations;
9. criar índices;
10. registrar versão padrão;
11. continuar para a tela inicial.

A operação deverá ser:

- idempotente;
- transacional quando possível;
- recuperável;
- segura contra interrupção.

---

# 12. IMPORTAÇÃO MANUAL DE NOVAS VERSÕES

O usuário deverá conseguir adicionar futuras traduções manualmente.

Crie tela:

```text
Configurações > Versões da Bíblia
```

Ações:

```text
Adicionar versão
Validar
Ativar/desativar
Definir como padrão
Ver detalhes
Remover versão importada
```

Ao clicar em **Adicionar versão**:

1. abrir seletor de arquivo;
2. aceitar SQLite compatível;
3. copiar para área privada do aplicativo;
4. nunca trabalhar permanentemente usando o arquivo externo original;
5. abrir em modo seguro;
6. validar integridade;
7. validar esquema;
8. ler metadata;
9. detectar código/nome;
10. detectar colisão;
11. validar referências;
12. mostrar resumo;
13. concluir importação;
14. atualizar catálogo.

Se for incompatível:

- não instalar parcialmente;
- mostrar motivo claro;
- manter aplicativo funcionando.

---

# 13. SERVIÇOS BÍBLICOS OBRIGATÓRIOS

Implemente interfaces e serviços para:

```text
IBibleRepository
IBibleService
IBibleVersionManager
IBibleValidationService
IBibleComparisonService
```

No mínimo:

```text
GetVersionsAsync()
GetActiveVersionAsync()
SetActiveVersionAsync()

GetBooksAsync(version)
GetChaptersAsync(version, bookId)
GetVersesAsync(version, bookId, chapter)

GetVerseAsync(version, reference)
GetPassageAsync(version, referenceRange)

SearchAsync(version, query)
CompareAsync(reference, versions)

ValidateAsync(version)
ImportVersionAsync(file)
RemoveImportedVersionAsync(version)
```

---

# 14. BANCO DO USUÁRIO

Crie o banco principal do aplicativo.

Utilize migrations/versionamento.

Entidades mínimas:

```text
Tema
ReferenciaBiblica
ReferenciaTema
Mensagem
MensagemTopico
MensagemReferencia
Configuracao
BibleVersionCatalog
```

Prepare extensão para:

```text
Favorito
Colecao
Tag
Historico
```

---

# 15. ENTIDADE TEMA

Campos mínimos:

```text
Id
Nome
CorHex
Descricao
DataCriacao
DataAlteracao
```

Regras:

- nome obrigatório;
- validar duplicidade conforme política definida;
- cor válida;
- exclusão não pode deixar relacionamentos órfãos.

Uma referência pode possuir vários temas.

Um tema pode possuir várias referências.

---

# 16. REFERÊNCIA SALVA

Entidade:

```text
ReferenciaBiblica
```

Campos:

```text
Id
BookId
Chapter
VerseStart
VerseEnd
Comentario
PreferredBibleVersionId
DataCriacao
DataAlteracao
```

Não use `BibleVerseId` como vínculo rígido com uma tradução.

---

# 17. RELAÇÃO REFERÊNCIA X TEMA

Entidade:

```text
ReferenciaTema
```

Campos:

```text
ReferenciaId
TemaId
```

Crie constraint única:

```text
ReferenciaId + TemaId
```

Não permitir duplicidade.

---

# 18. COMENTÁRIO PERMANENTE X OBSERVAÇÃO

Esta regra é **obrigatória**.

## 18.1. Comentário permanente

```text
ReferenciaBiblica.Comentario
```

É uma anotação geral sobre a referência.

Exemplo:

```text
João 3:16
"Texto central sobre o amor de Deus."
```

## 18.2. Observação contextual

```text
MensagemReferencia.Observacao
```

É específica para a utilização da referência dentro de uma mensagem.

Exemplo:

```text
"Usar na introdução."
```

## 18.3. Nunca misturar

- alterar comentário não altera observação;
- alterar observação não altera comentário;
- relatórios devem diferenciá-los;
- a mesma referência pode ter várias observações em várias mensagens.

---

# 19. MENSAGENS

Entidade:

```text
Mensagem
```

Campos:

```text
Id
Titulo
Descricao
Tipo
DataCriacao
DataAlteracao
```

Tipos:

```text
Mensagem
Pregacao
Estudo
Devocional
```

Utilize enum ou Value Object controlado.

---

# 20. TÓPICOS

Entidade:

```text
MensagemTopico
```

Campos:

```text
Id
MensagemId
Titulo
Conteudo
Ordem
```

Regras:

- permitir criar;
- editar;
- excluir;
- reordenar;
- persistir ordem.

---

# 21. REFERÊNCIAS DA MENSAGEM

Entidade:

```text
MensagemReferencia
```

Campos:

```text
Id
MensagemId
ReferenciaId
TopicoId?
Ordem
Observacao
PreferredBibleVersionId?
```

A referência pode estar:

- ligada à mensagem geral;
- ligada a um tópico.

A ordem deve ser editável.

---

# 22. DUPLICAÇÃO DE MENSAGENS

Ao duplicar:

- criar nova `Mensagem`;
- copiar propriedades;
- criar novos `MensagemTopico`;
- criar novos `MensagemReferencia`;
- reutilizar `ReferenciaBiblica`;
- copiar observações;
- recriar vínculos para os novos tópicos;
- nunca duplicar desnecessariamente uma referência já existente.

A duplicação deve ocorrer em transação.

---

# 23. TELAS OBRIGATÓRIAS

**TODAS AS TELAS ABAIXO DEVEM SER IMPLEMENTADAS.**

Não substituir telas por exemplos.

Não deixar telas vazias.

---

# 24. TELA INICIAL / DASHBOARD

Criar dashboard com:

- atalhos para Bíblia;
- nova mensagem;
- pesquisar;
- temas;
- relatórios;
- últimas mensagens;
- referências recentes;
- versão bíblica atual;
- acesso às configurações.

Desktop e celular deverão ter layouts adaptativos.

---

# 25. MÓDULO BÍBLIA

Criar telas:

```text
Bíblia > Ler
Bíblia > Pesquisar
Bíblia > Comparar versões
```

## 25.1. Leitor

Deverá conter:

- seletor de versão;
- seletor de livro;
- seletor de capítulo;
- conteúdo;
- navegação capítulo anterior/próximo;
- seleção de versículo;
- seleção de intervalo;
- copiar;
- salvar referência;
- adicionar a tema;
- adicionar a mensagem;
- comparar versões.

Ao trocar versão:

- manter livro;
- capítulo;
- referência/posição;
- sempre que houver correspondência.

---

# 26. PESQUISA BÍBLICA

Criar pesquisa por:

- palavra;
- expressão;
- livro;
- capítulo;
- versão;
- várias versões quando solicitado.

Resultado deve mostrar:

- referência;
- texto;
- versão;
- ações.

Ações:

- abrir na Bíblia;
- salvar referência;
- adicionar tema;
- adicionar à mensagem;
- comparar.

Implementar índices e/ou FTS5 quando adequado.

Não carregar toda a Bíblia em memória.

---

# 27. COMPARAÇÃO DE VERSÕES

Tela responsiva.

Permitir:

- escolher referência;
- selecionar 2 ou mais versões;
- visualizar textos.

Desktop:

- colunas quando houver espaço.

Mobile:

- cartões empilhados ou tabs próprias do aplicativo.

Mostrar:

- sigla;
- nome;
- texto;
- status de correspondência.

Se a referência estiver ausente:

```text
Referência não encontrada nesta versão.
```

Se houver ambiguidade:

```text
A versão possui mais de um registro para esta referência.
```

Nunca esconder divergência.

---

# 28. MÓDULO REFERÊNCIAS

Criar:

```text
Referências > Minhas referências
Referências > Nova referência
Referências > Editar referência
Referências > Detalhes
Referências > Pesquisar
```

---

# 29. CADASTRO DE REFERÊNCIA

Campos:

```text
Versão para visualização
Livro
Capítulo
Versículo inicial
Versículo final (opcional)
Referência formatada
Texto bíblico
Comentário permanente
Temas
```

O usuário **não deve digitar manualmente o texto bíblico** quando a referência existir.

Permitir seleção múltipla de temas.

---

# 30. MÓDULO TEMAS

Criar:

```text
Temas > Lista
Temas > Novo
Temas > Editar
Temas > Detalhes
Temas > Pesquisar
```

Cadastro:

```text
Nome
Cor
Descrição
```

Detalhes:

- referências associadas;
- quantidade;
- mensagens relacionadas;
- ações.

---

# 31. MÓDULO MENSAGENS

Criar:

```text
Mensagens > Todas
Mensagens > Nova
Mensagens > Editar
Mensagens > Detalhes
Mensagens > Pregações
Mensagens > Estudos
Mensagens > Devocionais
Mensagens > Pesquisar
```

Editor deve permitir:

- título;
- descrição;
- tipo;
- tópicos;
- ordenação;
- conteúdo;
- referências;
- referências por tópico;
- observações;
- escolha da versão preferida;
- salvar;
- duplicar;
- visualizar relatório;
- gerar PDF.

---

# 32. EDITOR DE MENSAGEM

Desktop:

- estrutura com painel de tópicos;
- editor;
- painel de referências ou ações acessíveis;
- visualização ampla.

Mobile:

- fluxo em etapas;
- tabs próprias ou navegação interna;
- não sacrificar funcionalidades.

Permitir reordenar:

- tópicos;
- referências.

Não depender obrigatoriamente de drag-and-drop; fornecer alternativa por botões se necessário.

---

# 33. PESQUISA GLOBAL

Criar pesquisa para:

- texto bíblico;
- referência;
- tema;
- comentário permanente;
- observação;
- título da mensagem;
- descrição;
- conteúdo de tópico.

Permitir filtros combináveis.

Separar internamente:

- pesquisa bíblica;
- pesquisa do banco do usuário.

---

# 34. RELATÓRIOS

Criar módulo completo:

```text
Relatórios
├── Mensagem
├── Pregações
├── Estudos
├── Devocionais
├── Mensagens por tema
├── Referências por tema
├── Referências com comentários
├── Histórico de utilização
├── Temas
└── Referências
```

---

# 35. RELATÓRIO DE MENSAGEM

Criar layout profissional estilo livro.

Estrutura:

```text
TÍTULO

Descrição

TEMAS

INTRODUÇÃO

TÓPICO 1
Título
Conteúdo

Referência bíblica
Texto bíblico
Comentário permanente
Observação contextual

TÓPICO 2
...

CONCLUSÃO

REFERÊNCIAS UTILIZADAS
```

Exibir:

- versão usada;
- créditos/licença quando exigido.

---

# 36. PDF

Implementar serviço:

```text
IReportService
IPdfService
```

Requisitos:

- geração offline;
- A4;
- margens apropriadas;
- tipografia profissional;
- títulos;
- subtítulos;
- referências destacadas;
- paginação;
- quebra de páginas;
- cabeçalho/rodapé quando necessário;
- evitar títulos órfãos;
- impressão legível;
- salvar;
- compartilhar.

O modelo do relatório deve ser independente da UI.

---

# 37. BACKUP

Criar tela:

```text
Configurações > Backup e Restauração
```

Funções:

- criar backup;
- escolher destino quando suportado;
- restaurar;
- validar arquivo;
- mostrar data;
- mostrar versão do esquema.

O backup deverá incluir os dados do usuário.

Não precisa incluir os bancos bíblicos empacotados por padrão.

---

# 38. RESTAURAÇÃO

Antes de restaurar:

- validar arquivo;
- validar schema version;
- criar cópia de segurança;
- usar transação;
- impedir restauração parcial.

Se falhar:

- restaurar estado anterior;
- informar erro.

---

# 39. CONFIGURAÇÕES

Criar telas/seções:

```text
Configurações
├── Aparência
├── Versões da Bíblia
├── Bíblia padrão
├── Backup e Restauração
├── Preferências
└── Informações / Licenças
```

---

# 40. TEMA VISUAL

Implementar:

- tema claro;
- tema escuro;
- recursos centralizados;
- cores centralizadas;
- estilos reutilizáveis;
- tipografia consistente.

Não deixar cores fixas espalhadas em Views.

Usar:

```text
Resources/Styles/Colors.xaml
Resources/Styles/Styles.xaml
```

ou organização equivalente.

---

# 41. UI NÃO NATIVA / CONSISTÊNCIA

Todos os elementos visuais do domínio do aplicativo devem seguir o design system próprio.

Criar componentes próprios para:

- cards;
- headers;
- botões;
- campos;
- mensagens de erro;
- empty states;
- dialogs quando apropriado;
- confirmação;
- chips/tags;
- seletor de tema;
- indicador de versão;
- loading;
- navegação interna.

Quando um componente de sistema operacional for inevitável, por exemplo file picker, sua aparência externa poderá ser do SO, porém ele deve estar isolado em serviço e não definir a identidade visual do aplicativo.

---

# 42. RESPONSIVIDADE

Use mecanismos MAUI adequados:

- `VisualStateManager`;
- `AdaptiveTrigger`;
- layouts flexíveis;
- Grid;
- componentes reutilizáveis.

Celular:

- coluna;
- cartões;
- navegação compacta;
- comandos claros.

Desktop:

- menu lateral;
- listas + detalhes;
- editores amplos;
- filtros persistentes;
- preview.

---

# 43. ACESSIBILIDADE

Aplicar:

- `SemanticProperties`;
- labels;
- descrição de botões;
- foco;
- contraste;
- área de toque adequada;
- tamanhos de fonte;
- navegação por teclado onde possível.

---

# 44. NAVEGAÇÃO PRINCIPAL

Sugestão mínima:

```text
Início

Bíblia
├── Ler
├── Pesquisar
└── Comparar

Referências
├── Minhas referências
├── Nova
└── Pesquisar

Temas

Mensagens
├── Todas
├── Nova
├── Pregações
├── Estudos
└── Devocionais

Relatórios

Configurações
```

Implemente navegação real.

---

# 45. PADRÃO MVVM

Cada tela relevante deverá ter ViewModel.

Não inserir regra de negócio em code-behind.

Code-behind somente para comportamento estritamente visual quando necessário.

Utilize:

- ObservableProperty ou equivalente;
- Commands;
- serviços injetados;
- cancellation token quando apropriado.

---

# 46. BANCO E REPOSITÓRIOS

Views:

**NUNCA acessam SQLite.**

ViewModels:

**NUNCA contêm SQL bruto.**

Repositórios:

- encapsulam persistência.

Serviços:

- coordenam regras.

Domain:

- regras puras.

---

# 47. TRANSAÇÕES

Utilizar transação em:

- duplicação de mensagem;
- restauração;
- criação que envolva várias tabelas;
- exclusões compostas;
- importação/registro de versão quando aplicável;
- migrations críticas.

---

# 48. EXCLUSÕES

Mensagem:

- excluir tópicos;
- excluir vínculos;
- não excluir referências compartilhadas.

Tema:

- remover relações;
- não deixar órfãos.

Referência:

- verificar política;
- remover relações;
- proteger integridade.

Versão importada:

- não excluir banco empacotado obrigatório;
- não quebrar referências salvas;
- referências são canônicas e independentes da versão.

---

# 49. SEGURANÇA

Obrigatório:

- SQL parametrizado;
- não concatenar entrada do usuário;
- validar arquivos importados;
- validar caminhos;
- não executar conteúdo de arquivo;
- limitar SQLite às operações esperadas;
- logs sem dados sensíveis desnecessários.

---

# 50. DESEMPENHO

Obrigatório:

- operações assíncronas;
- não carregar Bíblia inteira;
- paginação/virtualização quando necessário;
- índices;
- FTS quando adequado;
- abrir conexões sob demanda;
- liberar conexão;
- cache limitado para livros/metadados;
- cancelamento em pesquisas longas.

Avaliar índice:

```sql
CREATE INDEX IF NOT EXISTS idx_verse_reference
ON verse(book_id, chapter, verse);
```

Não alterar bancos distribuídos sem uma estratégia controlada.

---

# 51. TESTES OBRIGATÓRIOS

Crie projetos de teste.

## 51.1. Unitários

Testar:

- referência;
- formatação;
- intervalos;
- validações;
- comentários x observações;
- duplicação;
- ordenação;
- regras de exclusão.

## 51.2. Integração

Testar:

- AppDatabase;
- migrations;
- repositories;
- BibleRepository;
- leitura das versões;
- importação;
- validação;
- comparação.

## 51.3. Validação das Bíblias

Para cada banco:

- integrity_check;
- esquema;
- metadata;
- livros;
- capítulos;
- versículos;
- duplicidades;
- FKs;
- referência conhecida.

---

# 52. TESTES DE UI / FLUXO

Ao menos validar manualmente ou automatizar quando viável:

```text
Abrir app
Selecionar versão
Abrir João 3
Abrir João 3:16
Trocar versão
Salvar referência
Criar tema
Associar tema
Criar mensagem
Criar tópico
Adicionar João 3:16
Adicionar observação
Salvar
Reabrir
Duplicar
Gerar relatório
Gerar PDF
Criar backup
Restaurar
Importar nova versão
Comparar versões
```

---

# 53. TRATAMENTO DE ERROS

Crie estratégia central.

Erros amigáveis:

- banco ausente;
- banco inválido;
- versão incompatível;
- referência ausente;
- duplicidade;
- backup inválido;
- falha de restauração;
- erro PDF;
- erro de leitura;
- falta de permissão.

Nunca encerrar silenciosamente.

---

# 54. ESTADOS DE UI

Toda tela que carrega dados deverá considerar:

```text
Loading
Success
Empty
Error
```

Não deixar tela branca sem explicação.

---

# 55. FASES DE IMPLEMENTAÇÃO

Execute **exatamente nesta ordem**.

---

# FASE 0 — AUDITORIA DO PROJETO

1. abrir solução;
2. analisar estrutura;
3. confirmar .NET 10;
4. confirmar targets;
5. compilar projeto original;
6. registrar baseline;
7. listar warnings;
8. criar documentação inicial.

### Critério de conclusão

```text
dotnet build
```

deve terminar sem erro.

---

# FASE 1 — AUDITORIA DAS BÍBLIAS

1. examinar `D:\Projetos\Biblia\Versoes`;
2. listar bancos;
3. abrir cada um;
4. validar integridade;
5. identificar metadata;
6. validar schema;
7. registrar anomalias;
8. criar relatório em `docs/BANCOS_BIBLICOS.md`.

Não corrigir dados silenciosamente.

### Critério

Todas as versões devem estar classificadas como:

```text
Compatível
Compatível com ressalva
Incompatível
```

---

# FASE 2 — FUNDAÇÃO DA ARQUITETURA

Criar:

- camadas;
- DI;
- logging;
- abstrações;
- Domain;
- Application;
- Infrastructure;
- Presentation;
- Tests.

Compilar.

---

# FASE 3 — BANCO DO APLICATIVO

Criar:

- schema;
- entidades;
- constraints;
- migrations;
- initializer;
- repositories.

Testar CRUD.

---

# FASE 4 — SISTEMA DE VERSÕES

Criar:

- BibleVersion;
- catálogo;
- VersionManager;
- manifesto;
- instalação inicial;
- versão padrão;
- validação.

---

# FASE 5 — EMPACOTAMENTO DAS BÍBLIAS

Criar estratégia de build/publicação para incluir versões aprovadas da pasta:

```text
D:\Projetos\Biblia\Versoes
```

Não depender dessa pasta no dispositivo do usuário.

Implementar cópia na primeira execução.

Testar reinstalação/idempotência.

---

# FASE 6 — IMPORTAÇÃO MANUAL

Implementar tela e serviço para novas versões.

Testar:

- banco válido;
- duplicado;
- corrompido;
- esquema errado.

---

# FASE 7 — BIBLE REPOSITORY

Implementar consultas comuns.

Testes contra várias traduções.

---

# FASE 8 — LEITOR BÍBLICO

Criar UI completa.

Testar mobile e desktop.

---

# FASE 9 — PESQUISA BÍBLICA

Implementar busca real.

Medir consultas.

---

# FASE 10 — COMPARAÇÃO

Implementar comparação multi-versão.

Testar referências divergentes.

---

# FASE 11 — TEMAS

CRUD completo.

Pesquisa.

Detalhes.

---

# FASE 12 — REFERÊNCIAS SALVAS

CRUD completo.

Intervalos.

Comentários.

Temas.

---

# FASE 13 — MENSAGENS

CRUD completo.

Tipos.

Pesquisa.

---

# FASE 14 — TÓPICOS

Criar editor.

Ordenação.

---

# FASE 15 — REFERÊNCIAS NA MENSAGEM

Implementar vínculos.

Observação contextual.

Ordenação.

Tópicos.

---

# FASE 16 — DUPLICAÇÃO

Implementar em transação.

Testes obrigatórios.

---

# FASE 17 — PESQUISA GLOBAL

Implementar filtros combináveis.

---

# FASE 18 — RELATÓRIOS

Criar modelos e previews.

---

# FASE 19 — PDF

Criar exportação profissional.

Validar PDF real.

---

# FASE 20 — BACKUP E RESTAURAÇÃO

Implementar e testar.

---

# FASE 21 — CONFIGURAÇÕES

Finalizar:

- aparência;
- versões;
- backup;
- preferências;
- informações;
- licenças.

---

# FASE 22 — RESPONSIVIDADE

Revisar todas as páginas em:

- celular;
- tablet;
- desktop.

Não considerar fase concluída se uma tela estiver apenas esticada.

---

# FASE 23 — TEMA CLARO/ESCURO

Revisar todas as páginas e componentes.

Não deixar cores ilegíveis.

---

# FASE 24 — ACESSIBILIDADE

Revisar todas as telas.

---

# FASE 25 — TESTES COMPLETOS

Executar:

```text
dotnet restore
dotnet build
dotnet test
```

e comandos específicos necessários.

Corrigir erros.

---

# FASE 26 — TESTE DE FLUXO COMPLETO

Executar fluxo funcional completo do usuário.

Documentar resultado.

---

# FASE 27 — DOCUMENTAÇÃO FINAL

Atualizar:

```text
README.md
AGENTS.md
docs/*
```

README deverá conter:

- objetivo;
- arquitetura;
- pré-requisitos;
- build;
- run;
- testes;
- bancos;
- importação;
- publicação.

---

# 56. CHECKPOINT OBRIGATÓRIO APÓS CADA FASE

Ao concluir cada fase, registre:

```text
FASE:
STATUS:
ARQUIVOS CRIADOS:
ARQUIVOS ALTERADOS:
TESTES EXECUTADOS:
RESULTADO:
WARNINGS:
PENDÊNCIAS:
PRÓXIMA FASE:
```

Se houver pendência crítica:

```text
STATUS: BLOQUEADA
```

Corrija antes de seguir.

---

# 57. PROIBIÇÕES

Você **NÃO PODE**:

- pular fase;
- deixar implementação simulada;
- entregar apenas arquitetura;
- criar telas sem funcionalidade;
- criar botão sem comando;
- ignorar erro;
- esconder exceção;
- usar IDs de `verse` para correlacionar traduções;
- misturar banco bíblico com banco do usuário;
- colocar SQL em View;
- colocar SQL bruto em ViewModel;
- colocar regra de negócio em code-behind;
- depender de `D:\Projetos\Biblia\Versoes` em produção;
- sobrescrever banco do usuário durante atualização;
- apagar comentários ao alterar observações;
- apagar observações ao alterar comentários;
- exigir internet para funções do núcleo;
- criar interface completamente diferente por plataforma;
- substituir funções pedidas por TODO;
- concluir o projeto sem executar build e testes.

---

# 58. POLÍTICA DE DECISÕES

Quando houver uma pequena decisão técnica não especificada:

- escolha a opção mais simples;
- mantenha arquitetura limpa;
- documente em `docs/DECISOES.md`;
- prossiga.

Não interrompa o desenvolvimento por perguntas triviais.

Solicite decisão do usuário somente quando existir impacto real sobre:

- licença;
- perda de dados;
- requisito contraditório;
- custo externo;
- chave/API externa;
- comportamento irreversível.

---

# 59. LICENÇAS DAS BÍBLIAS

Antes de considerar uma tradução apta à distribuição:

- documente origem;
- documente licença;
- documente permissão.

Se a licença não estiver comprovada:

- não remova o suporte técnico;
- classifique a versão como não aprovada para distribuição;
- mantenha possibilidade de importação manual se juridicamente apropriado;
- registre a pendência.

Nunca presumir que um arquivo encontrado localmente pode ser redistribuído.

---

# 60. ATUALIZAÇÃO DO APLICATIVO

Atualizações futuras não podem:

- perder dados;
- apagar banco do usuário;
- sobrescrever importações pessoais sem consentimento;
- resetar temas;
- resetar mensagens;
- resetar comentários.

Bíblias empacotadas devem ter estratégia de versão.

---

# 61. CRITÉRIOS DE ACEITAÇÃO DO PRODUTO

O projeto só será considerado concluído quando:

1. compilar;
2. testes passarem;
3. abrir normalmente;
4. primeira instalação carregar as versões aprovadas;
5. permitir importação manual;
6. listar versões;
7. trocar versão;
8. ler Bíblia;
9. pesquisar Bíblia;
10. comparar versões;
11. criar tema;
12. editar tema;
13. excluir tema corretamente;
14. criar referência;
15. associar vários temas;
16. editar comentário;
17. criar mensagem;
18. criar tópicos;
19. adicionar referências;
20. registrar observações;
21. reordenar;
22. duplicar mensagem;
23. pesquisar conteúdo;
24. gerar relatórios;
25. gerar PDF;
26. criar backup;
27. restaurar backup;
28. funcionar offline;
29. funcionar em layouts mobile e desktop;
30. suportar claro/escuro;
31. não apresentar erros de integridade conhecidos;
32. possuir documentação.

---

# 62. DEFINIÇÃO DE PRONTO PARA CADA TELA

Uma tela só está pronta se:

- abrir;
- carregar;
- exibir loading;
- exibir empty state;
- exibir erro;
- validar entrada;
- executar comandos;
- persistir alterações;
- atualizar estado;
- navegar corretamente;
- obedecer tema claro;
- obedecer tema escuro;
- funcionar em celular;
- funcionar em desktop;
- ter acessibilidade básica.

---

# 63. DEFINIÇÃO DE PRONTO PARA CADA SERVIÇO

Um serviço só está pronto se:

- interface definida;
- implementação registrada em DI;
- validações;
- cancellation token quando pertinente;
- tratamento de erro;
- logging apropriado;
- teste unitário/integrado;
- documentação mínima.

---

# 64. DEFINIÇÃO DE PRONTO PARA BANCO BÍBLICO

Uma versão só é considerada utilizável se:

- arquivo existir;
- abrir;
- integrity_check OK;
- esquema reconhecido;
- metadata legível;
- livros legíveis;
- versículos pesquisáveis;
- anomalias registradas;
- catálogo atualizado.

---

# 65. EXPERIÊNCIA FINAL ESPERADA

Exemplo de fluxo:

1. instalar aplicativo;
2. primeira execução instala bancos bíblicos;
3. aplicação abre com versão padrão;
4. usuário acessa Bíblia;
5. seleciona `1 Coríntios`;
6. capítulo `4`;
7. seleciona `1-6`;
8. aplicação mostra texto automaticamente;
9. usuário salva referência;
10. associa:
    - Sabedoria;
    - Ministério;
    - Serviço;
    - Liderança;
11. registra comentário permanente;
12. cria mensagem:
    - `Servindo com Fidelidade`;
13. cria tópico:
    - `Responsabilidade no ministério`;
14. adiciona referência;
15. registra observação:
    - `Usar para explicar que o ministro deve ser encontrado fiel.`;
16. salva;
17. troca NVI por ARA;
18. referência permanece;
19. visualiza tradução;
20. compara traduções;
21. gera relatório;
22. gera PDF;
23. fecha;
24. reabre;
25. todos os dados continuam íntegros.

Esse fluxo deverá funcionar de verdade.

---

# 66. SAÍDA DO CODEX DURANTE A EXECUÇÃO

Não produza longas explicações abstratas antes de implementar.

A cada fase:

1. informe resumidamente o que vai fazer;
2. execute;
3. mostre arquivos relevantes;
4. compile/teste;
5. corrija;
6. registre checkpoint;
7. avance.

---

# 67. PRIMEIRA AÇÃO A EXECUTAR AGORA

Comece imediatamente pela **FASE 0 — AUDITORIA DO PROJETO**.

Depois execute a **FASE 1 — AUDITORIA DAS BÍBLIAS** usando:

```text
D:\Projetos\Biblia\Versoes
```

Não comece criando telas antes de validar:

- arquitetura atual;
- build;
- bancos bíblicos;
- forma de empacotamento;
- banco principal.

Depois continue as fases rigorosamente na ordem deste documento.

---

# 68. ORDEM FINAL INEGOCIÁVEL

```text
AUDITAR
↓
VALIDAR
↓
MODELAR
↓
IMPLEMENTAR FUNDAÇÃO
↓
IMPLEMENTAR DADOS
↓
IMPLEMENTAR SERVIÇOS
↓
IMPLEMENTAR TELAS
↓
IMPLEMENTAR PESQUISAS
↓
IMPLEMENTAR RELATÓRIOS
↓
IMPLEMENTAR PDF
↓
IMPLEMENTAR BACKUP
↓
TESTAR
↓
CORRIGIR
↓
DOCUMENTAR
↓
VALIDAR PRODUTO COMPLETO
```

**NÃO PULE NENHUMA ETAPA.**

**NÃO CONSIDERE O PROJETO CONCLUÍDO ENQUANTO QUALQUER ITEM DE ACEITAÇÃO ESTIVER PENDENTE.**

---

# 69. INSTRUÇÃO FINAL

Você recebeu autorização para modificar o projeto existente e criar os arquivos necessários para implementar o BibliaTema conforme esta especificação.

Trabalhe diretamente sobre o projeto.

Faça alterações reais.

Execute os comandos necessários.

Compile frequentemente.

Teste frequentemente.

Corrija problemas antes de avançar.

Preserve dados.

Preserve a arquitetura.

Respeite todas as regras de negócio.

Mantenha a aplicação offline-first.

Mantenha o suporte multi-versão desacoplado.

Implemente todas as telas de cadastro.

Implemente todas as telas de pesquisa.

Implemente todos os relatórios definidos.

Implemente backup/restauração.

Implemente instalação inicial das Bíblias.

Implemente importação manual de novas versões.

Não simplifique silenciosamente requisitos.

Não substitua funcionalidades obrigatórias por placeholders.

**COMECE AGORA PELA FASE 0.**
