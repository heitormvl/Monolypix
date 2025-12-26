using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Monolypix.Models;

namespace Monolypix.ViewModels;

public class DistributeInitialBalanceViewModel
{
    public Guid GameSessionId { get; set; }

    public string GameSessionName { get; set; } = string.Empty;

    public List<User> Players { get; set; } = new();
}
