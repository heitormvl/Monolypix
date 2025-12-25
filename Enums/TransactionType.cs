using System.ComponentModel.DataAnnotations;

namespace Monolypix.Enums;

public enum TransactionType
{
    [Display(Name = "Transferência PIX entre Usuários")]
    PixTransfer,
    [Display(Name = "Crédito do Banco")]
    BankCredit,
    [Display(Name = "Débito do Banco")]
    BankDebit,
    [Display(Name = "Multa")]
    Fine,
    [Display(Name = "Bônus")]
    Bonus,
    [Display(Name = "Crédito Inicial")]
    InitialCredit
}
