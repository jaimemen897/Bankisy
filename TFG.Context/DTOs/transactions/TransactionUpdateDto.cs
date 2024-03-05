namespace TFG.Context.DTOs.transactions;

public class TransactionUpdateDto
{
    public string? Concept { get; set; }
    public decimal? Amount { get; set; }
    public Guid? IdAccountOrigin { get; set; }
    public Guid? IdAccountDestination { get; set; }
    public DateTime? Date { get; set; }
}