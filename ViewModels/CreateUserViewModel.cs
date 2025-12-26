using System.ComponentModel.DataAnnotations;

namespace Monolypix.ViewModels;

public class CreateUserViewModel
{
    [Required]
    [MaxLength(50)]
    [Display(Name = "Nome do Usuário")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome do usuário deve ter entre 3 e 50 caracteres.")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "Cor do Avatar")]
    [MaxLength(7)]
    [RegularExpression("^#([A-Fa-f0-9]{6})$")]
    public string AvatarColor { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Banqueiro")]
    public bool IsBanker { get; set; } = false;

    [Required]
    [Display(Name = "Sessão de Jogo")]
    public Guid GameSessionId { get; set; }
}
