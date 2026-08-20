# Auditoria das Bíblias — Fase 1

Data: 18/08/2026  
Origem auditada: `D:\Projetos\Biblia\Versoes`

## Método

Cada arquivo foi aberto em modo somente leitura e submetido a:

- `PRAGMA quick_check` e `PRAGMA foreign_key_check`;
- inventário de schema e metadata;
- contagem de livros, versículos e referências distintas;
- verificação de nulos/vazios, números inválidos, órfãos e duplicidades de `(book_id, chapter, verse)`;
- validação dos identificadores de livros/testamentos;
- comparação das coordenadas com ACF como referência técnica, sem presumir que toda divergência de versificação seja corrupção;
- cálculo de SHA-256 para rastreabilidade.

Nenhum banco foi modificado.

## Schema comum

Todos os bancos usam `dbversion=2` e o mesmo schema:

- `metadata(key VARCHAR(255) PRIMARY KEY, value VARCHAR(255))`;
- `book(id INTEGER PRIMARY KEY, book_reference_id INTEGER, testament_reference_id INTEGER, name VARCHAR(50))`;
- `verse(id INTEGER PRIMARY KEY, book_id INTEGER, chapter INTEGER, verse INTEGER, text TEXT, FOREIGN KEY (book_id) REFERENCES book(id))`.

Todos têm 66 livros, referências de livro 1–66, testamentos 1–2, zero órfãos, zero campos obrigatórios nulos/vazios e zero capítulos/versículos menores que 1. `quick_check` retornou `ok` e `foreign_key_check` retornou zero violações em todos.

O schema não possui índice nem restrição única sobre `(book_id, chapter, verse)`. Isso permite duplicidades e exigirá cuidado na camada de acesso; recomenda-se validar os arquivos na importação e indexar uma cópia de trabalho, sem alterar os originais silenciosamente.

## Classificação

| Arquivo | Metadata | Versículos / refs. distintas | Classificação | Observações |
|---|---|---:|---|---|
| ACF.sqlite | Almeida Corrigida e Fiel | 31.102 / 31.102 | Compatível | Sem anomalias detectadas. |
| ARA.sqlite | Almeida Revista e Atualizada | 31.104 / 31.104 | Compatível com ressalva | Sem duplicidades; diferenças de versificação frente à ACF: falta 2Co 13:14 e há 3 coordenadas adicionais. |
| ARC.sqlite | Almeida Revista e Corrigida | 31.105 / 31.105 | Compatível com ressalva | Sem duplicidades; diferenças de versificação frente à ACF: falta 2Co 13:14 e há 4 coordenadas adicionais. |
| AS21.sqlite | Almeida Século 21 | 31.104 / 31.104 | Compatível com ressalva | Sem duplicidades; diferenças de versificação frente à ACF: falta 2Co 13:14 e há 3 coordenadas adicionais. |
| KJF.sqlite | King James Fiel | 31.102 / 31.102 | Compatível | Sem anomalias detectadas. |
| NAA.sqlite | Nova Almeida Atualizada | 31.105 / 31.097 | Incompatível | Oito referências excedentes por duplicidade e textos deslocados entre capítulos/livros. |
| NBV.sqlite | Nova Bíblia Viva | 31.105 / 31.105 | Compatível com ressalva | Sem duplicidades; 3 coordenadas adicionais frente à ACF, compatíveis com variação de versificação a confirmar funcionalmente. |
| NTLH.sqlite | Nova Tradução na Linguagem de Hoje | 31.103 / 31.102 | Incompatível | Duplicidade em 2Sm 24:25 e sequência de 2Sm 23:25–39 gravada como capítulos 25–38. |
| NVI.sqlite | Nova Versão Internacional | 31.105 / 31.105 | Compatível com ressalva | Sem duplicidades; 3 coordenadas adicionais frente à ACF, compatíveis com variação de versificação a confirmar funcionalmente. |
| NVT.sqlite | Nova Versão Transformadora | 31.102 / 31.101 | Incompatível | Sl 119:13 duplicado; a segunda ocorrência contém o texto correspondente a Sl 119:130, que está ausente. |

## Anomalias detalhadas

### NAA.sqlite

Há pares de coordenadas duplicadas cujos textos pertencem a referências distintas:

- Sl 111:6–7 misturado com Sl 110:6–7;
- Is 5:5–6 misturado com Is 4:5–6;
- Is 13:5–6 misturado com Is 12:5–6;
- Os 4:4–5 misturado com Os 3:4–5.

Também foram observadas divergências isoladas de versificação em relação à ACF. As duplicidades acima, por si sós, tornam consultas por referência ambíguas.

### NTLH.sqlite

- `2 Samuel 24:25` aparece duas vezes: uma ocorrência contém um bloco marcado `[24-39]` com nomes dos valentes de 2Sm 23; a outra contém o texto esperado de 2Sm 24:25.
- As coordenadas 2Sm 23:25–39 não existem; os textos correspondentes aparecem deslocados como 2Sm 25:26, 26:27, ..., 38:39, produzindo capítulos inexistentes.

### NVT.sqlite

- Sl 119:13 aparece duas vezes.
- A segunda ocorrência diz “O ensinamento de tua palavra esclarece...”, texto associado a Sl 119:130.
- Sl 119:130 está ausente.

## Divergências de versificação não classificadas como corrupção

Comparar apenas a quantidade total com uma versão de referência não é suficiente: traduções podem separar, combinar ou omitir coordenadas por tradição editorial. Por isso ARA, ARC, AS21, NBV e NVI ficaram como **Compatível com ressalva**, e não incompatíveis, já que têm referências únicas e nenhuma inconsistência estrutural detectada. A aplicação deve tratar ausência de uma coordenada como dado possível e não assumir conjuntos idênticos entre versões.

## SHA-256

| Arquivo | SHA-256 |
|---|---|
| ACF.sqlite | `B9AF738EEA2F01785E47D612371DBB590D3641106AF768EE1A5961F62A6FED44` |
| ARA.sqlite | `EEDE75C2A2BEEB7E1A91BCD8DF9325422CFFCC1B3CC63E0BD65F49C5CC020964` |
| ARC.sqlite | `7427C8DA6EB1B99D7B500BBEEF73088ED84314146FCD9D4D0B92BB401DCFC212` |
| AS21.sqlite | `44C268F39802750AE495C9FCA09E2F03C471B6769410F62193CB22D94D202EAF` |
| KJF.sqlite | `DBEDA15D6A17A97AEB52A21C557F89C40190FAF32A2CDF96E36BFD45D1FD2DA5` |
| NAA.sqlite | `43EDFD42E27E9A535AAB7D5BBA19604EBD8C0966642BE177323F203FE29B31AD` |
| NBV.sqlite | `734F0D3F460A5CFF644B6EDE9E3EAB23D011FB75A1AD64180A2C86176BDFDBCA` |
| NTLH.sqlite | `F95714FA970B88642C7EDF0E89FADEF04380059A398A04C856CAFD2B62A5F167` |
| NVI.sqlite | `7F65865F3660EA5C7BEC30F8A7BB40D8D3D27A979755472CB2F00E16D0AA1198` |
| NVT.sqlite | `0B7746AE59970DADFC7307D0BAAB69D19AB3C44F345055EEC67AA2D0D60390BF` |

## Conclusão e bloqueio de avanço

A auditoria foi concluída e todos os bancos foram classificados. Entretanto, NAA, NTLH e NVT são **Incompatíveis** no estado atual por anomalias referenciais que podem retornar o texto bíblico errado.

Conforme a regra de não avançar diante de erro crítico e de não corrigir dados silenciosamente, **a Fase 2 não deve começar** até haver uma decisão explícita: substituir/corrigir esses três arquivos a partir de uma fonte autorizada e auditável, ou excluí-los do conjunto inicial de versões suportadas.
