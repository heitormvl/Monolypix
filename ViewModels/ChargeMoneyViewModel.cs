using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Monolypix.Models;

namespace Monolypix.ViewModels;

public class ChargeMoneyViewModel
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    [Precision(18, 2)]
    [Display(Name = "Valor")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(250, ErrorMessage = "A descrição não pode ultrapassar 250 caracteres.")]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    public Guid GameSessionId { get; set; }

    public List<User> Players { get; set; } = new();
}
