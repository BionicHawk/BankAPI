using BankAPI.Data;
using BankAPI.Data.BankModels;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Services;

public class BankTransactionService {
    private readonly BankContext _context;

    public BankTransactionService(BankContext context) => _context = context;

    public async Task<Client?> GetClient(string Email) {
        return await _context.Clients
            .SingleOrDefaultAsync(client => client.Email == Email);
    }

    public async Task<Account?> GetAccountByID(int id) {

        return await _context.Accounts.FindAsync(id);

    }

    public async Task<IEnumerable<Account?>> GetAccounts(Client client) {
        return await _context.Accounts
            .Where(a => a.ClientId == client.Id).ToListAsync();
    }

    public async Task<bool> TryToRetireWithTransfer(Account account, decimal amount, int? externalAccount) {

        if (account is null) return false;

        decimal newBalance = account.Balance - amount;

        if (newBalance < 0.0m) return false;

        var transaction = new BankTransaction() {
            AccountId = account.Id,
            TranstactionType = 4,
            Amount = amount,
            ExternalAccount = externalAccount
        };

        account.Balance = newBalance;
        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        return true;

    }

    public async Task<bool> TryToRetireWithCash(Account account, decimal amount) {

        if (account is null) return false;
        
        decimal newBalance = account.Balance - amount;

        if (newBalance < 0.0m) return false;

        var transaction = new BankTransaction() {
            AccountId = account.Id,
            TranstactionType = 2,
            Amount = amount
        };

        account.Balance = newBalance;
        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<bool> TryToDeposit(Account account, decimal balance) {

        if (account is null) return false;

        var transaction = new BankTransaction
        {
            AccountId = account.Id,
            TranstactionType = 1,
            Amount = balance
        };

        _context.BankTransactions.Add(transaction);
        account.Balance += balance;
        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<bool> TryDeleteAccount(Account account) {

        if (account.Balance != 0.0m) return false;

        var transactions = await _context.BankTransactions
            .Where(t => t.AccountId == account.Id).ToListAsync();

        foreach (var transaction in transactions) {
            _context.Remove(transaction);
        }
        
        _context.Remove(account);
        await _context.SaveChangesAsync();
        
        return true;

    }

}