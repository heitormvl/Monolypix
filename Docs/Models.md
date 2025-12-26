# Documentação dos Models

## Visão Geral

Este documento descreve os modelos de dados utilizados na aplicação Monolypix, um sistema de gerenciamento de transações financeiras para jogos de Monopoly.

---

## AppDbContext

**Namespace:** `Monolypix.Models`

### Descrição

Classe de contexto do Entity Framework Core que gerencia o acesso ao banco de dados e define as configurações das entidades.

### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Users` | `DbSet<User>` | Conjunto de usuários do sistema |
| `Wallets` | `DbSet<Wallet>` | Conjunto de carteiras dos jogadores |
| `Transactions` | `DbSet<Transaction>` | Conjunto de transações financeiras |
| `GameSessions` | `DbSet<GameSession>` | Conjunto de sessões de jogo |

### Configurações do Modelo

#### GameSession
- **Índice único:** Campo `Name` (nome da sessão)

#### Wallet
- **Índice único composto:** Combinação de `UserId` e `GameSessionId`
- **Relacionamentos:**
  - `User`: Relacionamento 1:N com restrição de exclusão
  - `GameSession`: Relacionamento 1:N com restrição de exclusão
- **Precisão:** Campo `Balance` com 18 dígitos (2 decimais)

#### Transaction
- **Relacionamentos:**
  - `FromWallet`: Carteira de origem com restrição de exclusão
  - `ToWallet`: Carteira de destino com restrição de exclusão
- **Índices:**
  - `GameSessionId`
  - `FromWalletId`
  - `ToWalletId`
- **Precisão:** Campo `Amount` com 18 dígitos (2 decimais)

---

## ErrorViewModel

**Namespace:** `Monolypix.Models`

### Descrição

Modelo de visualização para exibição de erros na aplicação.

### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `RequestId` | `string?` | Identificador da requisição que gerou o erro |
| `ShowRequestId` | `bool` | Indica se o RequestId deve ser exibido (true se não for nulo ou vazio) |

---

## GameSession

**Namespace:** `Monolypix.Models`

### Descrição

Representa uma sessão de jogo do Monopoly, permitindo múltiplas partidas independentes.

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `Id` | `Guid` | Sim | Identificador único da sessão |
| `Name` | `string` | Sim | Nome da sessão (3-20 caracteres, único) |
| `IsActive` | `bool` | Sim | Indica se a sessão está ativa (padrão: true) |
| `CreatedAt` | `DateTime` | Sim | Data/hora de criação (UTC) |
| `EndedAt` | `DateTime?` | Não | Data/hora de encerramento da sessão |

### Validações

- **Name:** 
  - Comprimento mínimo: 3 caracteres
  - Comprimento máximo: 20 caracteres
  - Deve ser único no sistema

### Índices

- Índice único em `Name`

---

## Result<T>

**Namespace:** `Monolypix.Models`

### Descrição

Classe genérica para encapsular o resultado de operações, incluindo sucesso/falha, mensagem e dados retornados.

### Propriedades

| Propriedade | Tipo | Descrição |
|-------------|------|-----------|
| `Success` | `bool` | Indica se a operação foi bem-sucedida |
| `Message` | `string` | Mensagem descritiva do resultado |
| `Data` | `T?` | Dados retornados pela operação (genérico) |

### Métodos Estáticos

#### Successful(T data, string message = "")
Cria um resultado de sucesso com os dados fornecidos.

**Parâmetros:**
- `data`: Dados a serem retornados
- `message`: Mensagem de sucesso (opcional)

**Retorno:** `Result<T>` com `Success = true`

#### Failure(string message)
Cria um resultado de falha com a mensagem de erro.

**Parâmetros:**
- `message`: Mensagem de erro

**Retorno:** `Result<T>` com `Success = false`

---

## Transaction

**Namespace:** `Monolypix.Models`

### Descrição

Representa uma transação financeira no sistema, podendo ser de diferentes tipos (crédito inicial, débito bancário, transferência Pix).

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `Id` | `Guid` | Sim | Identificador único da transação |
| `Type` | `TransactionType` | Sim | Tipo da transação (enum) |
| `FromWalletId` | `Guid?` | Não | ID da carteira de origem |
| `FromWallet` | `Wallet?` | Não | Navegação para carteira de origem |
| `ToWalletId` | `Guid?` | Não | ID da carteira de destino |
| `ToWallet` | `Wallet?` | Não | Navegação para carteira de destino |
| `Amount` | `decimal` | Sim | Valor da transação (18,2) |
| `Description` | `string?` | Não | Descrição da transação (máx. 250 caracteres) |
| `IsCompleted` | `bool` | Sim | Indica se a transação foi concluída (padrão: false) |
| `GameSessionId` | `Guid` | Sim | ID da sessão de jogo |
| `CreatedAt` | `DateTime` | Sim | Data/hora de criação (UTC) |
| `CompletedAt` | `DateTime?` | Não | Data/hora de conclusão |

### Validações

- **Amount:** Não pode ser negativo
- **Description:** Máximo de 250 caracteres

---

## CreateBankDebitRequestModel

**Namespace:** `Monolypix.Models`

### Descrição

Modelo para criação de solicitação de débito bancário.

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `GameSessionId` | `Guid` | Sim | ID da sessão de jogo |
| `Amount` | `decimal` | Sim | Valor do débito (18,2) |
| `Description` | `string?` | Não | Descrição do débito (máx. 250 caracteres) |

---

## CreateTransactionRequestModel

**Namespace:** `Monolypix.Models`

### Descrição

Modelo para criação de solicitação de transferência Pix (sem carteira de origem definida).

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `ToWalletId` | `Guid?` | Não | ID da carteira de destino |
| `Amount` | `decimal` | Sim | Valor da transação (18,2) |
| `Description` | `string?` | Não | Descrição da transação (máx. 250 caracteres) |

---

## CreateTransactionModel

**Namespace:** `Monolypix.Models`

### Descrição

Modelo para criação de transação completa com carteiras de origem e destino definidas.

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `FromWalletId` | `Guid?` | Não | ID da carteira de origem |
| `ToWalletId` | `Guid?` | Não | ID da carteira de destino |
| `Amount` | `decimal` | Sim | Valor da transação (18,2) |
| `Description` | `string?` | Não | Descrição da transação (máx. 250 caracteres) |

---

## User

**Namespace:** `Monolypix.Models`

### Descrição

Representa um jogador no sistema.

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `Id` | `Guid` | Sim | Identificador único do usuário |
| `UserName` | `string` | Sim | Nome do usuário (3-50 caracteres) |
| `AvatarColor` | `string` | Sim | Cor do avatar em formato hexadecimal (#RRGGBB) |
| `IsBanker` | `bool` | Sim | Indica se o usuário é o banqueiro (padrão: false) |
| `GameSessionId` | `Guid` | Sim | ID da sessão de jogo |
| `GameSession` | `GameSession?` | Não | Navegação para a sessão de jogo |

### Validações

- **UserName:** 
  - Comprimento mínimo: 3 caracteres
  - Comprimento máximo: 50 caracteres
- **AvatarColor:** 
  - Formato hexadecimal: `^#([A-Fa-f0-9]{6})$`
  - Exemplo: `#FF5733`

---

## Wallet

**Namespace:** `Monolypix.Models`

### Descrição

Representa a carteira financeira de um jogador em uma sessão de jogo específica.

### Propriedades

| Propriedade | Tipo | Requerido | Descrição |
|-------------|------|-----------|-----------|
| `Id` | `Guid` | Sim | Identificador único da carteira |
| `Balance` | `decimal` | Sim | Saldo atual (18,2, padrão: 0) |
| `UserId` | `Guid` | Sim | ID do usuário proprietário |
| `User` | `User?` | Não | Navegação para o usuário |
| `GameSessionId` | `Guid` | Sim | ID da sessão de jogo |
| `GameSession` | `GameSession?` | Não | Navegação para a sessão de jogo |

### Validações

- **Balance:** Não pode ser negativo

### Índices

- Índice único composto em `UserId` e `GameSessionId` (um usuário só pode ter uma carteira por sessão)

---

## Relacionamentos Entre Entidades

```
GameSession (1) ─────┬─── (N) User
                     │
                     └─── (N) Wallet
                     │
                     └─── (N) Transaction

User (1) ───────────── (N) Wallet

Wallet (1) ─────┬─── (N) Transaction (FromWallet)
                │
                └─── (N) Transaction (ToWallet)
```

### Regras de Negócio

1. **Unicidade de Sessões:** Cada sessão deve ter um nome único
2. **Unicidade de Carteiras:** Cada usuário pode ter apenas uma carteira por sessão
3. **Integridade Referencial:** Exclusões de usuários, carteiras e sessões são restritas quando há dependências
4. **Precisão Monetária:** Todos os valores monetários usam 18 dígitos com 2 casas decimais
5. **Sessões Ativas:** Transações só podem ser realizadas em sessões ativas
