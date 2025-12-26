using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolypix.Models;
using Monolypix.Services;
using Monolypix.ViewModels;

namespace Monolypix.Controllers;

public class TransactionController : Controller
{
    private readonly AppDbContext _context;
    private readonly TransactionService _transactionService;

    public TransactionController(AppDbContext context, TransactionService transactionService)
    {
        _context = context;
        _transactionService = transactionService;
    }

    // GET: Transaction/SendMoney/{userId}
    public async Task<IActionResult> SendMoney(Guid userId)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (targetUser == null)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Usuário não encontrado.";
            return RedirectToAction("Index", "Home");
        }

        var viewModel = new SendMoneyViewModel
        {
            ToUserId = userId,
            ToUserName = targetUser.UserName
        };

        return View(viewModel);
    }

    // POST: Transaction/SendMoney
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMoney(SendMoneyViewModel model)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Recarregar dados do destinatário para exibir na view em caso de erro
        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == model.ToUserId);

        if (targetUser == null)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Usuário não encontrado.";
            return RedirectToAction("Index", "Home");
        }

        model.ToUserName = targetUser.UserName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var currentUser = await _context.Users
            .Include(u => u.Wallet)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(currentUserId));

        if (currentUser?.Wallet == null)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Carteira do usuário não encontrada.";
            return View(model);
        }

        var targetWallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == model.ToUserId);

        if (targetWallet == null)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Carteira do destinatário não encontrada.";
            return View(model);
        }

        var transactionModel = new CreateTransactionModel
        {
            FromWalletId = currentUser.Wallet.Id,
            ToWalletId = targetWallet.Id,
            Amount = model.Amount,
            Description = model.Description
        };

        var result = await _transactionService.CreateTransactionAsync(transactionModel);

        if (!result.Success)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = result.Message;
            return View(model);
        }

        TempData["ResultSuccess"] = true;
        TempData["ResultMessage"] = $"Transferência de {model.Amount:N2} para {targetUser.UserName} realizada com sucesso!";
        return RedirectToAction("Index", "Home");
    }

    // GET: Transaction/Charge
    public async Task<IActionResult> Charge()
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(currentUserId));

        if (currentUser == null || !currentUser.IsBanker)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Apenas o banqueiro pode cobrar.";
            return RedirectToAction("Index", "Home");
        }

        var players = await _context.Users
            .Include(u => u.Wallet)
            .Where(u => u.GameSessionId == currentUser.GameSessionId)
            .ToListAsync();

        var viewModel = new ChargeMoneyViewModel
        {
            GameSessionId = currentUser.GameSessionId,
            Players = players
        };

        return View(viewModel);
    }

    // POST: Transaction/Charge
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Charge(ChargeMoneyViewModel model)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(currentUserId));

        if (currentUser == null || !currentUser.IsBanker)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Apenas o banqueiro pode cobrar.";
            return RedirectToAction("Index", "Home");
        }

        // Recarregar jogadores para exibir na view em caso de erro
        model.Players = await _context.Users
            .Include(u => u.Wallet)
            .Where(u => u.GameSessionId == currentUser.GameSessionId)
            .ToListAsync();
        model.GameSessionId = currentUser.GameSessionId;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var bankDebitModel = new CreateBankDebitRequestModel
        {
            GameSessionId = currentUser.GameSessionId,
            Amount = model.Amount,
            Description = model.Description
        };

        var result = await _transactionService.CreateBankDebitRequestAsync(bankDebitModel);

        if (!result.Success)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = result.Message;
            return View(model);
        }

        TempData["ResultSuccess"] = true;
        TempData["ResultMessage"] = $"Cobrança de {model.Amount:N2} criada com sucesso!";
        return RedirectToAction("Index", "Home");
    }

    // GET: Transaction/DistributeInitialBalance
    public async Task<IActionResult> DistributeInitialBalance()
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(currentUserId));

        if (currentUser == null || !currentUser.IsBanker)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Apenas o banqueiro pode distribuir saldo inicial.";
            return RedirectToAction("Index", "Home");
        }

        var gameSession = await _context.GameSessions
            .FirstOrDefaultAsync(gs => gs.Id == currentUser.GameSessionId);

        if (gameSession == null)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Sessão de jogo não encontrada.";
            return RedirectToAction("Index", "Home");
        }

        var players = await _context.Users
            .Include(u => u.Wallet)
            .Where(u => u.GameSessionId == currentUser.GameSessionId)
            .ToListAsync();

        var viewModel = new DistributeInitialBalanceViewModel
        {
            GameSessionId = currentUser.GameSessionId,
            GameSessionName = gameSession.Name,
            Players = players
        };

        return View(viewModel);
    }

    // POST: Transaction/DistributeInitialBalance
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DistributeInitialBalance(DistributeInitialBalanceViewModel model)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(currentUserId));

        if (currentUser == null || !currentUser.IsBanker)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Apenas o banqueiro pode distribuir saldo inicial.";
            return RedirectToAction("Index", "Home");
        }

        var wallets = await _context.Wallets
            .Where(w => w.GameSessionId == currentUser.GameSessionId)
            .ToListAsync();

        var errors = new List<string>();
        var successCount = 0;

        foreach (var wallet in wallets)
        {
            var result = await _transactionService.ApplyInitialCreditAsync(wallet.Id);
            if (result.Success)
            {
                successCount++;
            }
            else
            {
                // Ignora erros de "já creditado"
                if (!result.Message.Contains("já foi aplicado"))
                {
                    errors.Add(result.Message);
                }
            }
        }

        if (errors.Any())
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = string.Join("; ", errors);
        }
        else if (successCount > 0)
        {
            TempData["ResultSuccess"] = true;
            TempData["ResultMessage"] = $"Saldo inicial distribuído para {successCount} jogador(es)!";
        }
        else
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Todos os jogadores já receberam o saldo inicial.";
        }

        return RedirectToAction("Index", "Home");
    }
}
