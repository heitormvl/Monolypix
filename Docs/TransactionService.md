# Documentação do TransactionService

## Visão Geral

**Namespace:** `Monolypix.Services`

O `TransactionService` é responsável por gerenciar todas as operações financeiras no sistema Monolypix, incluindo créditos iniciais, débitos bancários e transferências entre jogadores.

---

## Constantes

| Nome | Valor | Descrição |
|------|-------|-----------|
| `InitialCreditAmount` | `1500m` | Valor do crédito inicial concedido a cada carteira |

---

## Construtor

```csharp
public TransactionService(AppDbContext context)
```

### Parâmetros
- `context`: Instância do `AppDbContext` para acesso ao banco de dados

---

## Métodos

### 1. ApplyInitialCreditAsync

```csharp
public async Task<Result<Transaction>> ApplyInitialCreditAsync(Guid walletId)
```

#### Descrição
Aplica um crédito inicial de 1500 monopolys à carteira especificada, se ainda não foi aplicado.

#### Parâmetros
- `walletId` (Guid): ID da carteira que receberá o crédito

#### Retorno
`Result<Transaction>` contendo:
- **Sucesso:** A transação de crédito inicial criada
- **Falha:** Mensagem de erro

#### Validações
1. Verifica se a carteira existe
2. Verifica se o crédito inicial já foi aplicado anteriormente

#### Comportamento
- Cria uma transação do tipo `InitialCredit`
- Adiciona 1500 ao saldo da carteira
- Marca a transação como concluída imediatamente
- Utiliza transação de banco de dados para garantir consistência

#### Possíveis Erros
- `"Carteira não encontrada."` - Quando o walletId não existe
- `"Crédito inicial já foi aplicado."` - Quando já existe uma transação de crédito inicial para a carteira

#### Exemplo de Uso
```csharp
var result = await transactionService.ApplyInitialCreditAsync(walletId);
if (result.Success)
{
    Console.WriteLine($"Crédito aplicado: {result.Data.Amount}");
}
```

---

### 2. CreateBankDebitRequestAsync

```csharp
public async Task<Result<Transaction>> CreateBankDebitRequestAsync(CreateBankDebitRequestModel model)
```

#### Descrição
Cria uma solicitação de débito bancário para uma sessão de jogo. Este débito será pago posteriormente por um jogador específico.

#### Parâmetros
- `model` (CreateBankDebitRequestModel):
  - `GameSessionId` (Guid): ID da sessão de jogo
  - `Amount` (decimal): Valor do débito
  - `Description` (string): Descrição do débito

#### Retorno
`Result<Transaction>` contendo:
- **Sucesso:** A transação de débito bancário criada (não concluída)
- **Falha:** Mensagem de erro

#### Validações
1. Verifica se a sessão de jogo existe
2. Valida que a descrição não está vazia
3. Valida que o valor é maior que zero

#### Comportamento
- Cria uma transação do tipo `BankDebit`
- A transação é criada com `IsCompleted = false`
- Não possui carteira de origem definida inicialmente
- Aguarda pagamento através do método `PayBankDebitAsync`

#### Possíveis Erros
- `"Sessão de jogo não encontrada."` - Quando o gameSessionId não existe
- `"A descrição do débito bancário é obrigatória."` - Quando a descrição está vazia
- `"O valor do débito bancário deve ser maior que zero."` - Quando o valor é ≤ 0

#### Exemplo de Uso
```csharp
var model = new CreateBankDebitRequestModel
{
    GameSessionId = sessionId,
    Amount = 200m,
    Description = "Imposto sobre propriedade"
};
var result = await transactionService.CreateBankDebitRequestAsync(model);
```

---

### 3. PayBankDebitAsync

```csharp
public async Task<Result<Transaction>> PayBankDebitAsync(Guid transactionId, Guid fromWalletId)
```

#### Descrição
Paga um débito bancário previamente criado, debitando o valor da carteira especificada.

#### Parâmetros
- `transactionId` (Guid): ID da transação de débito bancário a ser paga
- `fromWalletId` (Guid): ID da carteira que pagará o débito

#### Retorno
`Result<Transaction>` contendo:
- **Sucesso:** A transação atualizada e concluída
- **Falha:** Mensagem de erro

#### Validações
1. Verifica se a transação existe e é do tipo `BankDebit`
2. Verifica se a transação já foi concluída
3. Verifica se a carteira de origem existe
4. Verifica se há saldo suficiente na carteira
5. Verifica se a carteira pertence à mesma sessão de jogo
6. Verifica se a sessão de jogo está ativa

#### Comportamento
- Debita o valor da carteira de origem
- Atualiza a transação com `FromWalletId`, `IsCompleted = true` e `CompletedAt`
- Utiliza transação de banco de dados para garantir consistência

#### Possíveis Erros
- `"Transação não encontrada."` - Transação inexistente ou tipo incorreto
- `"Transação já foi concluída."` - Transação já foi paga anteriormente
- `"Carteira de origem não encontrada."` - fromWalletId inválido
- `"Saldo insuficiente na carteira de origem."` - Saldo < valor do débito
- `"A carteira de origem não pertence à mesma sessão de jogo da transação."` - Incompatibilidade de sessões
- `"A sessão de jogo está encerrada."` - Sessão inativa

#### Exemplo de Uso
```csharp
var result = await transactionService.PayBankDebitAsync(transactionId, walletId);
if (result.Success)
{
    Console.WriteLine("Débito bancário pago com sucesso");
}
```

---

### 4. CreateTransactionRequestAsync

```csharp
public async Task<Result<Transaction>> CreateTransactionRequestAsync(CreateTransactionRequestModel model)
```

#### Descrição
Cria uma solicitação de transferência Pix para uma carteira específica. A carteira de origem será definida posteriormente quando alguém pagar a solicitação.

#### Parâmetros
- `model` (CreateTransactionRequestModel):
  - `ToWalletId` (Guid?): ID da carteira de destino
  - `Amount` (decimal): Valor da transferência
  - `Description` (string): Descrição da transação

#### Retorno
`Result<Transaction>` contendo:
- **Sucesso:** A transação de transferência Pix criada (não concluída)
- **Falha:** Mensagem de erro

#### Validações
1. Verifica se a carteira de destino foi informada
2. Verifica se a carteira de destino existe
3. Valida que o valor é maior que zero
4. Valida que a descrição não está vazia
5. Verifica se a sessão de jogo da carteira está ativa

#### Comportamento
- Cria uma transação do tipo `PixTransfer`
- A transação é criada com `IsCompleted = false`
- `FromWalletId` é nulo inicialmente
- Aguarda pagamento através do método `PayTransactionRequestAsync`

#### Possíveis Erros
- `"A carteira de destino é obrigatória."` - toWalletId é nulo
- `"Carteira de destino não encontrada."` - toWalletId inválido
- `"O valor da transação deve ser maior que zero."` - valor ≤ 0
- `"A descrição da transação é obrigatória."` - descrição vazia
- `"A sessão de jogo associada à carteira de destino está inativa."` - Sessão encerrada

#### Exemplo de Uso
```csharp
var model = new CreateTransactionRequestModel
{
    ToWalletId = destinationWalletId,
    Amount = 500m,
    Description = "Aluguel de propriedade"
};
var result = await transactionService.CreateTransactionRequestAsync(model);
```

---

### 5. PayTransactionRequestAsync

```csharp
public async Task<Result<Transaction>> PayTransactionRequestAsync(Guid transactionId, Guid fromWalletId)
```

#### Descrição
Paga uma solicitação de transferência Pix previamente criada, realizando a transferência entre as carteiras.

#### Parâmetros
- `transactionId` (Guid): ID da solicitação de transferência a ser paga
- `fromWalletId` (Guid): ID da carteira que fará o pagamento

#### Retorno
`Result<Transaction>` contendo:
- **Sucesso:** A transação concluída com a transferência realizada
- **Falha:** Mensagem de erro

#### Validações
1. Verifica se a transação existe e é do tipo `PixTransfer`
2. Verifica se a transação ainda não tem carteira de origem
3. Verifica se a transação já foi concluída
4. Verifica se a carteira de origem existe
5. Verifica se origem e destino são diferentes
6. Verifica se há saldo suficiente na carteira de origem
7. Verifica se ambas as carteiras pertencem à mesma sessão
8. Verifica se a sessão de jogo está ativa
9. Verifica se a carteira de destino existe durante a transação

#### Comportamento
- Debita o valor da carteira de origem
- Credita o valor na carteira de destino
- Atualiza a transação com `FromWalletId`, `IsCompleted = true` e `CompletedAt`
- Utiliza transação de banco de dados com rollback em caso de erro

#### Possíveis Erros
- `"Transação não encontrada."` - Transação inexistente ou tipo incorreto
- `"Transação já possui carteira de origem."` - Já foi paga anteriormente
- `"Transação já foi concluída."` - Status de conclusão inconsistente
- `"Carteira de origem não encontrada."` - fromWalletId inválido
- `"A carteira de origem não pode ser a mesma que a carteira de destino."` - Transferência para si mesmo
- `"Saldo insuficiente na carteira de origem."` - Saldo < valor
- `"A carteira de origem não pertence à mesma sessão de jogo da transação."` - Incompatibilidade de sessões
- `"A sessão de jogo está encerrada."` - Sessão inativa
- `"Carteira de destino não encontrada."` - Erro durante execução da transação

#### Exemplo de Uso
```csharp
var result = await transactionService.PayTransactionRequestAsync(requestId, payerWalletId);
if (result.Success)
{
    Console.WriteLine($"Transferência realizada: {result.Data.Amount}");
}
```

---

### 6. CreateTransactionAsync

```csharp
public async Task<Result<Transaction>> CreateTransactionAsync(CreateTransactionModel model)
```

#### Descrição
Cria e executa imediatamente uma transação completa entre duas carteiras, sem necessidade de solicitação prévia.

#### Parâmetros
- `model` (CreateTransactionModel):
  - `FromWalletId` (Guid?): ID da carteira de origem
  - `ToWalletId` (Guid?): ID da carteira de destino
  - `Amount` (decimal): Valor da transação
  - `Description` (string): Descrição da transação

#### Retorno
`Result<Transaction>` contendo:
- **Sucesso:** A transação criada e imediatamente concluída
- **Falha:** Mensagem de erro

#### Validações
1. Verifica se ambas as carteiras foram informadas
2. Valida que o valor é maior que zero
3. Verifica se ambas as carteiras existem
4. Verifica se origem e destino são diferentes
5. Valida que a descrição não está vazia
6. Verifica se ambas as carteiras pertencem à mesma sessão
7. Verifica se a sessão de jogo está ativa
8. Verifica se há saldo suficiente na carteira de origem

#### Comportamento
- Valida todas as condições antes de iniciar a transação
- Debita o valor da carteira de origem
- Credita o valor na carteira de destino
- Cria uma transação do tipo `PixTransfer` já concluída
- Define `IsCompleted = true` e `CompletedAt` imediatamente
- Utiliza transação de banco de dados para garantir consistência

#### Possíveis Erros
- `"Carteiras são obrigatórias."` - Uma ou ambas as carteiras não foram informadas
- `"O valor da transação deve ser maior que zero."` - valor ≤ 0
- `"Carteira de origem ou destino não encontrada."` - IDs inválidos
- `"A carteira de origem não pode ser a mesma que a carteira de destino."` - Transferência para si mesmo
- `"A descrição da transação é obrigatória."` - descrição vazia
- `"As carteiras devem pertencer à mesma sessão de jogo."` - Incompatibilidade de sessões
- `"A sessão de jogo está encerrada."` - Sessão inativa
- `"Saldo insuficiente na carteira de origem."` - Saldo < valor

#### Exemplo de Uso
```csharp
var model = new CreateTransactionModel
{
    FromWalletId = senderWalletId,
    ToWalletId = receiverWalletId,
    Amount = 300m,
    Description = "Pagamento de propriedade"
};
var result = await transactionService.CreateTransactionAsync(model);
```

---

## Fluxos de Trabalho

### Fluxo 1: Crédito Inicial
1. Jogador cria carteira
2. Sistema aplica crédito inicial de 1500
3. Carteira está pronta para uso

### Fluxo 2: Débito Bancário (em duas etapas)
1. Banqueiro cria solicitação de débito bancário
2. Jogador específico paga o débito da sua carteira
3. Transação é concluída

### Fluxo 3: Transferência Pix - Solicitação (em duas etapas)
1. Jogador A cria solicitação de pagamento
2. Jogador B paga a solicitação
3. Valor é transferido de B para A

### Fluxo 4: Transferência Pix - Direta (uma etapa)
1. Jogador A transfere diretamente para Jogador B
2. Transação é concluída imediatamente

---

## Segurança e Consistência

### Transações de Banco de Dados
Todos os métodos que modificam saldos utilizam transações de banco de dados (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`) para garantir:
- **Atomicidade:** Todas as operações são concluídas ou nenhuma é aplicada
- **Consistência:** Os saldos sempre refletem transações válidas
- **Isolamento:** Operações concorrentes não causam inconsistências

### Validações de Segurança
- Verificação de sessão ativa antes de qualquer operação financeira
- Validação de saldo antes de débitos
- Garantia de que transações pertencem à mesma sessão de jogo
- Prevenção de auto-transferências
- Verificação de conclusão para evitar duplo pagamento

---

## Tipos de Transação

| Tipo | Enum | Descrição | FromWallet | ToWallet |
|------|------|-----------|------------|----------|
| Crédito Inicial | `InitialCredit` | Crédito de 1500 no início | null | Obrigatório |
| Débito Bancário | `BankDebit` | Pagamento ao banco | Definido no pagamento | null |
| Transferência Pix | `PixTransfer` | Transferência entre jogadores | Obrigatório | Obrigatório |

---

## Boas Práticas

1. **Sempre verificar Result.Success** antes de acessar os dados
2. **Tratar mensagens de erro** para feedback ao usuário
3. **Validar IDs** antes de chamar os métodos
4. **Verificar sessão ativa** antes de permitir operações
5. **Usar transações assíncronas** para melhor performance
6. **Não permitir operações** em sessões encerradas

---

## Dependências

- `Microsoft.EntityFrameworkCore` - Acesso ao banco de dados
- `Monolypix.Models.AppDbContext` - Contexto do Entity Framework
- `Monolypix.Models.Transaction` - Modelo de transação
- `Monolypix.Models.Wallet` - Modelo de carteira
- `Monolypix.Models.GameSession` - Modelo de sessão de jogo
- `Monolypix.Models.Result<T>` - Encapsulamento de resultados
- `Monolypix.Enums.TransactionType` - Tipos de transação
