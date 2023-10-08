namespace BankAPI.Data.DTOs;

public class RetireTransferDToIn {

    public int Id { get; set; }
    public decimal Amount { get; set; }
    public int? ExternalAccount { get; set; }

}