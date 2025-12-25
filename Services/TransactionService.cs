using System;
using Microsoft.EntityFrameworkCore;
using Monolypix.Models;
using Monolypix.Enums;

namespace Monolypix.Services;

public class TransactionService
{
    private const decimal InitialCreditAmount = 1500m;
    private readonly AppDbContext _context;

    public TransactionService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Applies an initial credit of 1500 monopolys to the specified wallet if not already applied.
    /// </summary>
    /// <param name="walletId"></param>
    /// <returns>
    /// A Result object containing the Transaction if successful, or an error message if not.
    /// </returns>
    public async Task<Result<Transaction>> ApplyInitialCreditAsync(Guid walletId)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId);

        if (wallet is null)
            return Result<Transaction>.Failure("Carteira não encontrada.");

        var alreadyCredited = await _context.Transactions.AnyAsync(t =>
            t.ToWalletId == walletId &&
            t.Type == Enums.TransactionType.InitialCredit);

        if (alreadyCredited)
            return Result<Transaction>.Failure("Crédito inicial já foi aplicado.");

        using var tx = await _context.Database.BeginTransactionAsync();

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.InitialCredit,
            ToWalletId = walletId,
            GameSessionId = wallet.GameSessionId,
            Amount = InitialCreditAmount,
            Description = "Crédito inicial",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        wallet.Balance += InitialCreditAmount;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        return Result<Transaction>.Successful(
            transaction,
            "Crédito inicial aplicado com sucesso."
        );
    }

    /// <summary>
    /// Creates a bank debit request transaction.
    /// </summary>
    /// <param name="gameSessionId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns>
    /// A Result object containing the Transaction if successful, or an error message if not.
    /// </returns>
    public async Task<Result<Transaction>> CreateBankDebitRequestAsync(CreateBankDebitRequestModel model)
    {
        var gameSessionId = model.GameSessionId;
        var amount = model.Amount;
        var description = model.Description;
        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == gameSessionId);
        
        if (gameSession is null)
            return Result<Transaction>.Failure("Sessão de jogo não encontrada.");
        if (string.IsNullOrWhiteSpace(description))
            return Result<Transaction>.Failure("A descrição do débito bancário é obrigatória.");
        if (amount <= 0)
            return Result<Transaction>.Failure("O valor do débito bancário deve ser maior que zero.");
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.BankDebit,
            GameSessionId = gameSessionId,
            Amount = amount,
            Description = description,
            IsCompleted = false
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return Result<Transaction>.Successful(
            transaction,
            "Solicitação de débito bancário criada com sucesso."
        );
    }

    /// <summary>
    /// Pays a bank debit transaction from the specified wallet.
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="fromWalletId"></param>
    /// <returns>
    /// A Result object containing the Transaction if successful, or an error message if not.
    /// </returns>
    public async Task<Result<Transaction>> PayBankDebitAsync(Guid transactionId, Guid fromWalletId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.Type == Enums.TransactionType.BankDebit);

        if (transaction is null)
            return Result<Transaction>.Failure("Transação não encontrada.");
        if (transaction.IsCompleted)
            return Result<Transaction>.Failure("Transação já foi concluída.");
        var fromWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == fromWalletId);
        if (fromWallet is null)
            return Result<Transaction>.Failure("Carteira de origem não encontrada.");
        if (fromWallet.Balance < transaction.Amount)
            return Result<Transaction>.Failure("Saldo insuficiente na carteira de origem.");
        if (fromWallet.GameSessionId != transaction.GameSessionId)
            return Result<Transaction>.Failure("A carteira de origem não pertence à mesma sessão de jogo da transação.");
        var sessionActive = await _context.GameSessions
            .AnyAsync(gs => gs.Id == fromWallet.GameSessionId && gs.IsActive);
        if (!sessionActive)
            return Result<Transaction>.Failure("A sessão de jogo está encerrada.");
        using var tx = await _context.Database.BeginTransactionAsync();
        fromWallet.Balance -= transaction.Amount;
        transaction.FromWalletId = fromWalletId;
        transaction.IsCompleted = true;
        transaction.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return Result<Transaction>.Successful(
            transaction,
            "Débito bancário pago com sucesso."
        );
    }

    /// <summary>
    /// Creates a Pix transfer transaction request.
    /// </summary>
    /// <param name="toWalletId"></param>
    /// <param name="amount"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public async Task<Result<Transaction>> CreateTransactionRequestAsync(CreateTransactionRequestModel model)
    {
        var toWalletId = model.ToWalletId;
        var amount = model.Amount;
        var description = model.Description;
        if (toWalletId is null)
            return Result<Transaction>.Failure("A carteira de destino é obrigatória.");
        var toWallet = await _context.Wallets.FirstOrDefaultAsync(w => w.Id == toWalletId);

        if (toWallet is null)
            return Result<Transaction>.Failure("Carteira de destino não encontrada.");
        if (amount <= 0)
            return Result<Transaction>.Failure("O valor da transação deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(description))
            return Result<Transaction>.Failure("A descrição da transação é obrigatória.");
        var sessionActive = await _context.GameSessions
            .AnyAsync(gs => gs.Id == toWallet.GameSessionId && gs.IsActive);
        if (!sessionActive)
            return Result<Transaction>.Failure("A sessão de jogo associada à carteira de destino está inativa.");
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.PixTransfer,
            FromWalletId = null,
            ToWalletId = toWalletId,
            Amount = amount,
            Description = description,
            IsCompleted = false,
            GameSessionId = toWallet.GameSessionId
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return Result<Transaction>.Successful(
            transaction,
            "Solicitação de transferência Pix criada com sucesso."
        );
    }

    /// <summary>
    /// Pays a Pix transfer transaction request from the specified wallet.
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="fromWalletId"></param>
    /// <returns></returns>
    public async Task<Result<Transaction>> PayTransactionRequestAsync(Guid transactionId, Guid fromWalletId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.Type == Enums.TransactionType.PixTransfer);

        if (transaction is null)
            return Result<Transaction>.Failure("Transação não encontrada.");
        if (transaction.FromWalletId is not null)
            return Result<Transaction>.Failure("Transação já possui carteira de origem.");
        if (transaction.IsCompleted)
            return Result<Transaction>.Failure("Transação já foi concluída.");
        var fromWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == fromWalletId);
        if (fromWallet is null)
            return Result<Transaction>.Failure("Carteira de origem não encontrada.");
        if (fromWallet.Id == transaction.ToWalletId)
            return Result<Transaction>.Failure("A carteira de origem não pode ser a mesma que a carteira de destino.");
        if (fromWallet.Balance < transaction.Amount)
            return Result<Transaction>.Failure("Saldo insuficiente na carteira de origem.");
        if (fromWallet.GameSessionId != transaction.GameSessionId)
            return Result<Transaction>.Failure("A carteira de origem não pertence à mesma sessão de jogo da transação.");
        var sessionActive = await _context.GameSessions
            .AnyAsync(gs => gs.Id == fromWallet.GameSessionId && gs.IsActive);
        if (!sessionActive)
            return Result<Transaction>.Failure("A sessão de jogo está encerrada.");
        using var tx = await _context.Database.BeginTransactionAsync();
        fromWallet.Balance -= transaction.Amount;
        var toWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == transaction.ToWalletId);
        if (toWallet is null)
        {
            await tx.RollbackAsync();
            return Result<Transaction>.Failure("Carteira de destino não encontrada.");
        }
        toWallet.Balance += transaction.Amount;
        transaction.FromWalletId = fromWalletId;
        transaction.IsCompleted = true;
        transaction.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return Result<Transaction>.Successful(
            transaction,
            "Transferência Pix realizada com sucesso."
        );
    }

    public async Task<Result<Transaction>> CreateTransactionAsync(CreateTransactionModel model)
    {
        var fromWalletId = model.FromWalletId;
        var toWalletId = model.ToWalletId;
        var amount = model.Amount;
        var description = model.Description;
        if (fromWalletId is null || toWalletId is null)
            return Result<Transaction>.Failure("Carteiras são obrigatórias.");
        
        if (amount <= 0)
            return Result<Transaction>.Failure("O valor da transação deve ser maior que zero.");
        var fromWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == fromWalletId);
        var toWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == toWalletId);
        if (fromWallet is null || toWallet is null)
            return Result<Transaction>.Failure("Carteira de origem ou destino não encontrada.");
        if (fromWallet.Id == toWallet.Id)
            return Result<Transaction>.Failure("A carteira de origem não pode ser a mesma que a carteira de destino.");
        if (string.IsNullOrWhiteSpace(description))
            return Result<Transaction>.Failure("A descrição da transação é obrigatória.");
        if (fromWallet.GameSessionId != toWallet.GameSessionId)
            return Result<Transaction>.Failure("As carteiras devem pertencer à mesma sessão de jogo.");
        var sessionActive = await _context.GameSessions
            .AnyAsync(gs => gs.Id == fromWallet.GameSessionId && gs.IsActive);
        if (!sessionActive)
            return Result<Transaction>.Failure("A sessão de jogo está encerrada.");
        if (fromWallet.Balance < amount)
            return Result<Transaction>.Failure("Saldo insuficiente na carteira de origem.");
        using var tx = await _context.Database.BeginTransactionAsync();
        fromWallet.Balance -= amount;
        toWallet.Balance += amount;
        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Type = TransactionType.PixTransfer,
            FromWalletId = fromWalletId,
            ToWalletId = toWalletId,
            Amount = amount,
            Description = description,
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow,
            GameSessionId = fromWallet.GameSessionId
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        await tx.CommitAsync();
        return Result<Transaction>.Successful(
            transaction,
            "Transação criada com sucesso."
        );
    }
}
