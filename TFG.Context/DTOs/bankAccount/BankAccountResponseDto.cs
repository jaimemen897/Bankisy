namespace TFG.Context.DTOs.bankAccount;

public class BankAccountResponseDto
{
    public string Iban { get; set; }
    public decimal Balance { get; set; }
    public string AccountType { get; set; }
    public List<Guid> UsersId { get; set; }
    public List<string> UsersName { get; set; }
    public bool IsDeleted { get; set; }
    public bool AcceptBizum { get; set; }
}