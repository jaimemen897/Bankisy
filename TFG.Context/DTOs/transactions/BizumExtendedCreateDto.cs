namespace TFG.Context.DTOs.transactions;

public class BizumExtendedCreateDto
{
    public string Concept { get; set; }
    public decimal Amount { get; set; }
    public string PhoneNumberUserOrigin { get; set; }
    public string IbanAccountOrigin { get; set; }
    public string IbanAccountDestination { get; set; }
    public string PhoneNumberUserDestination { get; set; }
}