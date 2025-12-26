using System;
using System.ComponentModel.DataAnnotations;

namespace Monolypix.ViewModels;

public class CreateGameSessionViewModel
{
    [Required(ErrorMessage = "O nome da sessão é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public required string Name { get; set; }
}
