using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolypix.Models;
using Monolypix.ViewModels;
using System.Text.Json;

namespace Monolypix.Controllers;


public class GameSessionsController : Controller
{
    private readonly AppDbContext _context;

    public GameSessionsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: GameSessionsController
    public ActionResult Index()
    {
        var gameSessions = _context.GameSessions.ToList();

        var viewModels = gameSessions.Select(gs => new ListGameSessionsViewModel
        {
            Id = gs.Id,
            Name = gs.Name,
            IsActive = gs.IsActive,
            CreatedAt = gs.CreatedAt.ToLocalTime(),
            EndedAt = gs.EndedAt.HasValue ? gs.EndedAt.Value.ToLocalTime() : (DateTime?)null,
            PlayerCount = _context.Users.Count(u => u.GameSessionId == gs.Id)
        }).ToList();

        return View(viewModels);
    }

    // GET: GameSessionsController/Create
    public ActionResult Create()
    {
        return View();
    }

    // POST: GameSessionsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(CreateGameSessionViewModel model)
    {
        if (ModelState.IsValid)
        {
            var gameSession = new GameSession
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.GameSessions.Add(gameSession);
            _context.SaveChanges();

            TempData["ResultSuccess"] = true;
            TempData["ResultMessage"] = $"Sessão \"{model.Name}\" criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        TempData["ResultSuccess"] = false;
        TempData["ResultMessage"] = "Por favor, preencha todos os campos obrigatórios.";
        return View(model);
    }

    // GET: GameSessionsController/Details/5
    public ActionResult Details(Guid id)
    {
        var gameSession = _context.GameSessions.Find(id);
        if (gameSession == null)
        {
            return NotFound();
        }

        var viewModel = new DetailGameSessionViewModel
        {
            Id = gameSession.Id,
            Name = gameSession.Name,
            IsActive = gameSession.IsActive,
            CreatedAt = gameSession.CreatedAt.ToLocalTime(),
            EndedAt = gameSession.EndedAt.HasValue ? gameSession.EndedAt.Value.ToLocalTime() : (DateTime?)null,
            Players = _context.Users
                .Include(u => u.Wallet)
                .Where(u => u.GameSessionId == gameSession.Id)
                .ToList()
        };
        return View(viewModel);
    }
}
