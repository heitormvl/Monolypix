using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Monolypix.ViewModels;

public class SendMoneyViewModel
{
    [Required]
    [Display(Name = "Destinatário")]
    public Guid ToUserId { get; set; }

    public string ToUserName { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    [Precision(18, 2)]
    [Display(Name = "Valor")]
    public decimal Amount { get; set; }

    [MaxLength(250, ErrorMessage = "A descrição não pode ultrapassar 250 caracteres.")]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;
}
