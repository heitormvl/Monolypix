using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Monolypix.Models;

[Index(nameof(Name), IsUnique = true)]
public class GameSession
{
    [Key]
    public Guid Id { get; set; }
    [Required]
    [MaxLength(20)]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "O nome da sessão precisa ter entre 3 e 20 caracteres.")]
    [Display(Name = "Nome da Sessão")]
    public string Name { get; set; } = string.Empty;
    [Required]
    [Display(Name = "Sessão Ativa")]
    public bool IsActive { get; set; } = true;
    [Required]
    [Display(Name = "Criado Em")]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Display(Name = "Encerrado Em")]
    [DataType(DataType.DateTime)]
    public DateTime? EndedAt { get; set; }
}
