using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Monolypix.Models;
using Monolypix.ViewModels;

namespace Monolypix.Controllers;

public class UsersController : Controller
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Create
    public IActionResult Create(Guid gameSessionId)
    {
        var model = new CreateUserViewModel
        {
            GameSessionId = gameSessionId
        };
        return View(model);
    }

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Por favor, preencha todos os campos obrigatórios.";
            return View(model);
        }

        var bankerAlreadyExists = _context.Users
            .Any(u => u.GameSessionId == model.GameSessionId && u.IsBanker);

        if (model.IsBanker && bankerAlreadyExists)
        {
            TempData["ResultSuccess"] = false;
            TempData["ResultMessage"] = "Já existe um banqueiro nesta sessão de jogo.";
            return View(model);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = model.UserName,
            AvatarColor = model.AvatarColor,
            IsBanker = model.IsBanker,
            GameSessionId = model.GameSessionId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        CreateWallet(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("GameSessionId", user.GameSessionId.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home", new { id = user.GameSessionId });
    }

    private void CreateWallet(User user)
    {
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            GameSessionId = user.GameSessionId
        };
        
        _context.Wallets.Add(wallet);
        _context.SaveChanges();
    }
}
