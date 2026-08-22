# Estúdio de cards de versículos

## Fluxo

`Home → Criar card → VerseCardStudioPage → IVerseCardService → PNG temporário → IFileTransferService`.

O Studio abre preenchido com o Versículo do Dia, sem modificar `DailyVerseService`. Também permite escolher versão instalada, livro canônico, capítulo e intervalo diretamente por `IBibleRepository`. Temas são carregados por `IThemeService`; quando uma referência guardada equivalente existe, seu primeiro tema é sugerido.

## Fotos e Pexels

`INatureMediaService` isola a UI da origem de mídia. `PexelsNatureMediaService` usa `GET https://api.pexels.com/v1/search`, orientação portrait, 15 itens e header `Authorization` quando há proxy/chave configurado. A chave nunca está no código: desenvolvimento usa `BIBLIATEMA_PEXELS_API_KEY`; produção deve configurar `BIBLIATEMA_MEDIA_BASE_URL` para a WR Soft Media API/proxy. Sem essa configuração, o aplicativo oferece um catálogo remoto curado hospedado no CDN do Pexels, além dos gradientes locais. Apenas o termo público de busca é enviado.

De seis a oito miniaturas são baixadas e validadas como JPEG em `CacheDirectory/verse-cards/previews` antes de aparecerem. A galeria usa HTML local em `WebView` e incorpora os arquivos validados como `data:` URLs; assim, a pré-visualização não depende do handler Android para caminhos privados. O toque é interceptado pelo esquema interno `bibliatema://photo/{id}` e atualiza a seleção no ViewModel.

“Carregar mais fotos” solicita páginas sucessivas de oito itens, acrescenta somente IDs ainda não exibidos e mantém os lotes anteriores. Ao tocar numa miniatura, uma nova fonte HTML local é atribuída à prévia para garantir o redesenho imediato no Android.

Resultados são mantidos em memória e a imagem final é armazenada em `CacheDirectory/verse-cards/backgrounds`. HTTP 401, 429, timeout, arquivo incompleto e falta de rede geram mensagem clara. Quatro gradientes locais mantêm o Studio funcional offline. Atribuição do fotógrafo e Pexels é preservada no modelo e no card.

Todos os seletores do Studio usam painel temático próprio; não dependem dos diálogos nativos do Android. Normal: `#FFFFFF/#E0E0E0`; selecionado: `#E4EDF7/#B8D4F0/#0066CC`.

## Renderização e compartilhamento

`SkiaVerseCardService` renderiza PNG dedicado em 1080×1350, 1080×1920 ou 1080×1080, com Open Sans incorporada, quebra por palavras, overlay de 30–60%, branding e crédito. Não é screenshot da página. Arquivos em cache são temporários; Salvar abre o FileSaver e Compartilhar abre o Share Sheet do sistema.
