# Arquitetura do BibliaTema

## Direção de dependências

`Presentation -> Application -> Domain` e `Infrastructure -> Application -> Domain`.

Enquanto o código permanecer no projeto MAUI único, essa direção é aplicada por namespaces, pastas, interfaces e revisão. A suíte de testes de domínio compila os fontes puros sem depender do workload MAUI.

## Camadas

- **Domain:** entidades, value objects, enums, regras e exceções puras.
- **Application:** contratos, DTOs, validações, casos de uso e orquestração independente da UI.
- **Infrastructure:** SQLite, arquivos, relógio, repositórios, backup, relatórios e logging técnico.
- **Presentation:** Views/ViewModels MAUI, componentes, conversores e navegação.
- **Tests:** testes unitários, integração, validação de bancos bíblicos e aplicação.

## Persistência

Há dois limites de dados independentes: bancos bíblicos somente leitura e o banco do aplicativo para dados do usuário e catálogo. As referências entre versões usam `(book_reference_id, chapter, verse)`, nunca `verse.id`.

Os originais em `D:\Projetos\Biblia\Versoes` não são alterados. A instalação futura copiará somente versões aprovadas para a área privada do aplicativo.

## Infraestrutura transversal

- DI centralizada em `AddApplication`, `AddInfrastructure` e `AddPresentation`.
- Logging via `Microsoft.Extensions.Logging`, com provider de debug em builds `DEBUG`.
- Diretórios privados abstraídos por `IAppPaths`, com validação de nome de arquivo.
- Tempo abstraído por `IClock` para testes determinísticos.
- Navegação abstraída por `IAppNavigator`, ocultando o Shell dos ViewModels.
