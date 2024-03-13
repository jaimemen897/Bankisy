namespace TFG.Context.DTOs.bankAccount;

public class BankAccountResponseDto
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }
    public string AccountType { get; set; }
    public List<Guid> UsersId { get; set; }
    public bool IsDeleted { get; set; }
}