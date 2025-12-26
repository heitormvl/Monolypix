using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Monolypix.Models;
using Monolypix.ViewModels;

namespace Monolypix.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return View();
        }

        var user = _context.Users
            .Include(u => u.Wallet)
            .FirstOrDefault(u => u.Id == Guid.Parse(userId));
        
        if (user == null)
        {
            return View();
        }

        var gameSession = _context.GameSessions
            .FirstOrDefault(gs => gs.Id == user.GameSessionId);

        if (gameSession == null)
        {
            return View();
        }

        var playersInSession = _context.Users
            .Include(u => u.Wallet)
            .Where(u => u.GameSessionId == gameSession.Id && u.Id != user.Id)
            .ToList();

        var viewModel = new IndexViewModel {
            Player = user,
            GameSession = gameSession,
            PlayersInSession = playersInSession
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}