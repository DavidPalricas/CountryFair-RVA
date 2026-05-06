Atua como um senior Unity game developer especializado em VR (Meta Quest, XR Interaction Toolkit) com C#.

## Tarefa
Atualiza e completa a documentação XML dos scripts C# do projeto, seguindo as normas padrão de documentação Unity/C#.

## Regras de documentação
- Usa `/// <summary>`, `/// <param>`, `/// <returns>`, `/// <remarks>` conforme adequado
- Documenta todos os campos `[SerializeField]` e propriedades públicas com `/// <summary>`
- Para métodos Unity (Awake, Start, Update, etc.), documenta o propósito específico no contexto do script, não o comportamento genérico
- Não documenta o óbvio — o comentário deve acrescentar contexto, não repetir o nome

## Ficheiros Inspector-driven
Para scripts com eventos ligados no Inspector (UnityEvent, botões UI, XR Interactable callbacks):
- Adiciona `/// <remarks>Invocado via Inspector em [NomePrefab/Cena]</remarks>` nos métodos públicos sem chamada direta no código
- Identifica esses métodos procurando assinaturas públicas sem referências internas

## Escopo
- Altera apenas ficheiros `.cs` indicados
- Não refatora lógica, não renomeia variáveis
- Se documentação existente estiver correta, mantém sem alteração
- Indica no final quais métodos públicos parecem ser Inspector-driven e precisam de confirmação