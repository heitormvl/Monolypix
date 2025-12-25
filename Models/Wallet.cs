using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Monolypix.Models;

[Index(nameof(UserId), nameof(GameSessionId), IsUnique = true)]
public class Wallet
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [Display(Name = "Saldo")]
    [Range(0, double.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
    [Precision(18, 2)]
    public decimal Balance { get; set; } = 0m;
    [Required]
    [Display(Name = "Usuário")]    
    public Guid UserId { get; set; }
    public User? User { get; set; }
    [Required]
    [Display(Name = "Sessão de Jogo")]    
    public Guid GameSessionId { get; set; }
    public GameSession? GameSession { get; set; }
}
