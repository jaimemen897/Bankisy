namespace TFG.Context.DTOs.transactions;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public string Concept { get; set; }
    public decimal Amount { get; set; }
    public string IbanAccountOrigin { get; set; }
    public string IbanAccountDestination { get; set; }
    public DateTime Date { get; set; }
}