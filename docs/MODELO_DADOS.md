# Modelo de dados do aplicativo

O banco `bibliatema.db` é criado na área privada do aplicativo. Ele é independente dos bancos bíblicos, que permanecem somente leitura.

## Versionamento

`SchemaMigration` registra cada migration aplicada. A versão atual é 1. A criação ocorre em transação e a inicialização é idempotente. Um banco com versão superior à suportada é recusado explicitamente.

## Tabelas

- `Theme`: temas com nome único sem distinção entre maiúsculas/minúsculas e cor hexadecimal opcional validada.
- `BibleVersionCatalog`: catálogo local das versões, estado de instalação/habilitação, licença e resultado de validação.
- `SavedReference`: referência canônica independente de tradução, comentário permanente e versão preferida opcional.
- `ReferenceTheme`: relação N:N com chave primária composta; duplicidade é proibida.
- `Message`: mensagem, pregação, estudo ou devocional.
- `MessageTopic`: tópicos ordenados, removidos em cascata com a mensagem.
- `MessageReference`: uso contextual de uma referência, com observação independente do comentário permanente.
- `Setting`: configurações chave/valor com upsert.

## Integridade

- Referências usam `BookReferenceId` de 1 a 66, capítulo e versículos positivos e `VerseEnd >= VerseStart`.
- Exclusão de tema ou referência remove `ReferenceTheme` em cascata.
- Exclusão de mensagem remove tópicos e vínculos da mensagem.
- Exclusão de referência é restringida enquanto estiver vinculada a uma mensagem.
- Exclusão de uma versão do catálogo define versões preferidas como nulas.
- Ordem é única dentro de tópicos e referências de cada mensagem.
- `PRAGMA foreign_keys=ON` é aplicado em toda conexão; WAL é habilitado na inicialização.

## Repositórios

SQL permanece exclusivamente em `Infrastructure/Repositories`. Views e ViewModels não acessam SQLite. Todos os valores variáveis usam parâmetros.
