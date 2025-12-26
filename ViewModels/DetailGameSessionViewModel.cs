using System;
using Monolypix.Models;

namespace Monolypix.ViewModels;

public class DetailGameSessionViewModel
{
    public Guid Id { get; set; }
    
    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public List<User> Players { get; set; } = new();
}
