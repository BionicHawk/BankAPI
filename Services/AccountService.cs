using Microsoft.EntityFrameworkCore;
using BankAPI.Data.BankModels;
using BankAPI.Data;
using BankAPI.Data.DTOs;

namespace BankAPI.Services;

public class AccountService {

    private readonly BankContext _context;

    public AccountService(BankContext context) => _context = context;

    public async Task<IEnumerable<AccountDToOut>> GetAll() {
        return await _context.Accounts.Select(a => new AccountDToOut{
            Id = a.Id,
            AccountName = a.AccountTypeNavigation != null? a.AccountTypeNavigation.Name : "",
            ClientName = a.Client != null?  a.Client.Name : "",
            Balance = a.Balance,
            RegDate = a.RegDate,
        }).ToListAsync();
    }

    public async Task<AccountDToOut?> GetDtoById(int id) {
        return await _context.Accounts
            .Where(a => a.Id == id)
            .Select(a => new AccountDToOut
        {
            Id = a.Id,
            AccountName = a.AccountTypeNavigation != null? a.AccountTypeNavigation.Name : "",
            ClientName = a.Client != null?  a.Client.Name : "",
            Balance = a.Balance,
            RegDate = a.RegDate,
        }).SingleOrDefaultAsync();
    }

    public async Task<Account?> GetById(int id) => await _context.Accounts.FindAsync(id);

    public async Task<Account> Create(AccountDToIn newAccount) {

        var account = new Account()
        {
            AccountType = newAccount.AccountType,
            ClientId = newAccount.ClientId,
            Balance = newAccount.Balance
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    public async Task Update(int id, AccountDToIn account) {

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
