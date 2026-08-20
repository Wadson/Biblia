# Regras de negócio — Mensagens e referências

O fluxo persistente é `Message → MessageReference → SavedReference → ReferenceTheme`. O texto bíblico permanece nos bancos de versões, que são somente leitura.

Uma `SavedReference` é reutilizada pela chave canônica livro, capítulo, versículo inicial e versículo final. Temas novos são unidos aos existentes. Comentário permanente pertence à referência salva; observação contextual pertence apenas ao vínculo com a mensagem.

Um vínculo com a mesma mensagem, referência e tópico não pode ser criado duas vezes. Remover o vínculo não remove a referência salva nem seus temas. A versão de consulta é local ao seletor; as preferências de mensagem e vínculo são persistidas separadamente.
