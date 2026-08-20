# Guia de engenharia do BibliaTema

## Arquitetura

O aplicativo usa .NET 10, .NET MAUI, MVVM e DI. Preserve as camadas `Domain`, `Application`, `Infrastructure` e `Presentation`. O banco do usuário é separado dos bancos bíblicos. Bancos bíblicos são fontes somente leitura; nunca altere os originais em `Versoes`.

## Regras obrigatórias

- Views nunca acessam SQLite; ViewModels nunca contêm SQL bruto.
- Repositórios encapsulam persistência; serviços coordenam regras; Domain contém regras puras.
- Nunca correlacione traduções por `verse.id`; use livro canônico, capítulo e versículo.
- Trate ausência, duplicidade e ambiguidade de versificação explicitamente.
- Use SQL parametrizado, operações assíncronas e `CancellationToken` quando pertinente.
- Não introduza TODO essencial, mock em produção ou tratamento que esconda exceções críticas.
- Nunca misture dados do usuário nos bancos bíblicos.

## Comandos

```powershell
dotnet restore Biblia.slnx
dotnet build Biblia.slnx --nologo
dotnet test Tests\Biblia.Tests\Biblia.Tests.csproj --nologo
```

## Validação antes de concluir

Execute build sem erros, testes relacionados sem falhas e registre o checkpoint da fase em `docs/DECISOES.md`. Não avance de fase com erro crítico, dependência quebrada ou teste obrigatório falhando.
