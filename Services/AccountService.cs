using Microsoft.EntityFrameworkCore;
using BankAPI.Data.BankModels;
using BankAPI.Data;

namespace BankAPI.Services;

public class AccountService {

    private readonly BankContext _context;

    public AccountService(BankContext context) => _context = context;

    public async Task<IEnumerable<Account>> GetAll() => await _context.Accounts.ToListAsync();

    public async Task<Account?> GetById(int id) => await _context.Accounts.FindAsync(id);

    public async Task<Account> Create(Account newAccount) {

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        return newAccount;
    }

    public async Task Update(int id, Account account) {

        var accountToUpdate = await GetById(id);

        if (accountToUpdate is not null) {

            accountToUpdate.AccountType = account.AccountType;
            accountToUpdate.ClientId = account.ClientId;
            accountToUpdate.Balance = account.Balance;

            await _context.SaveChangesAsync();

        }

    }

    public async Task Delete(int id) {

        var match = await GetById(id);

        if (match is not null) {

            _context.Remove(match);
            await _context.SaveChangesAsync();

        }

    }

}
