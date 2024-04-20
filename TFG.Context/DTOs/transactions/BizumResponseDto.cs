namespace TFG.Context.DTOs.transactions;

public class BizumResponseDto
{
    public int Id { get; set; }
    public string Concept { get; set; }
    public decimal Amount { get; set; }
    public string PhoneNumberUserOrigin { get; set; }
    public string PhoneNumberUserDestination { get; set; }
    public DateTime Date { get; set; }
}