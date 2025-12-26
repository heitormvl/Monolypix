using System;
using Monolypix.Models;
namespace Monolypix.ViewModels;

public class IndexViewModel
{
    public User Player { get; set; }

    public GameSession GameSession { get; set; }

    public List<User> PlayersInSession { get; set; }
}
