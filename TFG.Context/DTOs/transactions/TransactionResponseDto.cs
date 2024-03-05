namespace TFG.Context.DTOs.transactions;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public string Concept { get; set; }
    public decimal Amount { get; set; }
    public Guid IdAccountOrigin { get; set; }
    public Guid IdAccountDestination { get; set; }
    public DateTime Date { get; set; }
}