# Relatórios e PDF

## Fluxo

`ReportsPage → ReportsViewModel → IReportService → SermonReport → IPdfService → cache/exports → IFileTransferService`.

`SermonReport` é um documento estruturado. Cada referência contém nome canônico do livro, referência formatada, passagem real, comentário permanente, observação contextual, versão e temas. A camada de apresentação e o PDF não consultam bancos para resolver nomes.

## Origens

- Mensagem cadastrada preserva `BuildMessageAsync(messageId)` e transforma tópicos/vínculos existentes no documento editorial.
- Tema(s) consulta `ReferenceTheme` por IDs parametrizados, resolve passagens na versão escolhida e não cria mensagens artificiais.

## PDF

O gerador usa `PDFsharp-MigraDoc` 6.2.4, pacote MIT compatível com .NET 10. Open Sans é incorporada para Unicode. MigraDoc fornece fluxo automático, quebras de página, cabeçalhos, rodapés e campos `Página X de Y`.

O arquivo gerado em `CacheDirectory/exports` é temporário. **Salvar em...** chama o FileSaver do sistema e **Compartilhar** abre o Share Sheet com o PDF anexado, sem integração específica com WhatsApp ou Telegram.
