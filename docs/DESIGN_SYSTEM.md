# Design System — Bíblia Tema Premium

## Identidade

O tema global combina cabeçalho azul-marinho (`BtHeader`), branding dourado (`BtGold`), fundo off-white (`BtAppBackground`), superfícies brancas e ações azuis (`BtPrimary`). O dourado é reservado à marca Premium e não representa ações comuns.

## Paleta

| Token | Uso | Valor Light |
|---|---|---|
| `BtHeader` | Shell e branding | `#0D1E30` |
| `BtGold` / `BtGoldSoft` | Marca Premium | `#D4AF37` / `#F3E5AB` |
| `BtAppBackground` | Fundo das páginas | `#F4F6F9` |
| `BtSurface` | Cards e campos | `#FFFFFF` |
| `BtPrimary` | Ações e ícones | `#0066CC` |
| `BtSelectedBackground` | Seleção | `#E4EDF7` |
| `BtSelectedBorder` | Borda selecionada | `#B8D4F0` |
| `BtTextPrimary` / `BtTextSecondary` | Texto | `#0A0A0A` / `#6C757D` |
| `BtBorder` | Bordas e divisores | `#E0E0E0` |

Estados semânticos usam `BtSuccess*`, `BtWarning*` e `BtDanger*`. O modo escuro usa `BtDarkBackground`, `BtDarkSurface`, `BtDarkBorder` e textos claros, mantendo azul-marinho, azul e dourado como identidade.

## Componentes

- Cards: `BtCardStyle`, raio 14, borda de 1px e sombra discreta.
- Seleção: `BtSelectedCardStyle` ou estado `Selected`, sempre fundo e borda azul-claro.
- Botões: `BtPrimaryButtonStyle`, `BtSecondaryButtonStyle`, `BtDangerButtonStyle`, `BtIconButtonStyle` e `BtCompactButtonStyle`.
- Inputs: `BtEntryContainerStyle`, `BtEditorContainerStyle`, `BtSearchContainerStyle` e `BtSelectionFieldStyle`.
- Chips e menus: `BtChipStyle`, `BtChipSelectedStyle`, `BtMenuItemStyle` e `BtMenuItemSelectedStyle`.
- Status: `BtStatusInfoStyle`, `BtStatusSuccessStyle`, `BtStatusWarningStyle` e `BtStatusErrorStyle`.
- Auxiliares: `BtBadgeStyle` e `BtDividerStyle`.

Os controles selecionáveis possuem estados `Normal`, `Selected`, `Pressed` e `Disabled`. Botões possuem `Normal`, `PointerOver`, `Pressed` e `Disabled`.

## Sheets, dialogs e loading

`AppSelectionSheet` oferece seleção visual customizada. `BtAlertDialog` e `BtConfirmDialog` oferecem alertas e confirmações com identidade própria. `BtLoadingOverlay` apresenta progresso em card central. Eles são componentes de Presentation e não alteram regras de negócio.

## Ícones

Os ícones de navegação são SVG monocromáticos em `BtPrimary`. O logo usa azul-marinho, branco e dourado. Emojis não fazem parte da iconografia do flyout.

## Espaçamento e raio

Espaçamentos: `BtSpacingXs` 4, `BtSpacingSm` 8, `BtSpacingMd` 12, `BtSpacingLg` 16 e `BtSpacingXl` 24. Raios: 8, 12, 16 e 20 para small, medium, large e dialog.

## Responsividade

`FlexLayout`, limites de largura e listas verticais mantêm uma coluna em 360–390dp e distribuem painéis no tablet/desktop. Seletores horizontais possuem rolagem própria; a página não depende de scroll horizontal.
# Home devocional

O card principal usa superfície, borda e sombra do design system, badge `BtGoldSoft`, referência `BtPrimary` e texto bíblico sem truncamento. No mobile, atalhos são exibidos em duas colunas; em largura ampla, Versículo do Dia e Acesso rápido ocupam colunas distintas.

