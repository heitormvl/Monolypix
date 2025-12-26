using System.ComponentModel.DataAnnotations;

namespace Monolypix.ViewModels;

public class ListGameSessionsViewModel
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public int PlayerCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? EndedAt { get; set; }

}
