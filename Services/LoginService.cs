using Microsoft.EntityFrameworkCore;
using BankAPI.Data.BankModels;
using BankAPI.Data;
using BankAPI.Data.DTOs;

namespace BankAPI.Services;

public class LoginService {

    private readonly BankContext _context;

    public LoginService(BankContext context) => _context = context;

    public async Task<Administrator?> GetAdmin(AdminDTO admin) {
        return await _context.Administrators
            .SingleOrDefaultAsync(x => x.Email == admin.Email && x.Pwd == admin.Pwd);
    }

    public async Task<Client?> GetClient(ClientDToIn client) {
        return await _context.Clients
            .SingleOrDefaultAsync(x => x.Email == client.Email && x.Pwd == client.Pwd);
    }

}