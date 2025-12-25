using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Monolypix.Enums;

namespace Monolypix.Models;

public class Transaction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Tipo de Transação")]
    public TransactionType Type { get; set; }

    [Display(Name = "Carteira de Origem")]
    public Guid? FromWalletId { get; set; }
    public Wallet? FromWallet { get; set; }
    
    [Display(Name = "Carteira de Destino")]
    public Guid? ToWalletId { get; set; }
    public Wallet? ToWallet { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "O valor da transação não pode ser negativo.")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    
    [MaxLength(250, ErrorMessage = "A descrição não pode ultrapassar 250 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
    
    [Display(Name = "Concluída")]
    public bool IsCompleted { get; set; } = false;
    
    [Required]
    [Display(Name = "Sessão de Jogo")]
    public Guid GameSessionId { get; set; }
    
    [Required]
    [Display(Name = "Criado Em")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Concluído Em")]
    public DateTime? CompletedAt { get; set; }
}

public class CreateBankDebitRequestModel
{
    [Required]
    [Display(Name = "Sessão de Jogo")]
    public Guid GameSessionId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "O valor da transação não pode ser negativo.")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    
    [MaxLength(250, ErrorMessage = "A descrição não pode ultrapassar 250 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
}

public class CreateTransactionRequestModel
{
    [Display(Name = "Carteira de Destino")]
    public Guid? ToWalletId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "O valor da transação não pode ser negativo.")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    
    [MaxLength(250, ErrorMessage = "A descrição não pode ultrapassar 250 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
}

public class CreateTransactionModel
{
    [Display(Name = "Carteira de Origem")]
    public Guid? FromWalletId { get; set; }
    
    [Display(Name = "Carteira de Destino")]
    public Guid? ToWalletId { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "O valor da transação não pode ser negativo.")]
    [Precision(18, 2)]
    public decimal Amount { get; set; }
    
    [MaxLength(250, ErrorMessage = "A descrição não pode ultrapassar 250 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
}