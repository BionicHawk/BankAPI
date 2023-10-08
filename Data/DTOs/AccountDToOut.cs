namespace BankAPI.Data.DTOs;

public class AccountDToOut {
    public int Id { get; set; }
    public string? AccountName { get; set;}
    public string? ClientName { get; set;}
    public decimal Balance { get; set; }

    public DateTime RegDate { get; set; }
}