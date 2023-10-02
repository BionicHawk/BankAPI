using BankAPI.Data;
using BankAPI.Data.BankModels;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Services;

public class ClientService {

    private readonly BankContext _context;

    public ClientService(BankContext context) => _context = context;

    public async Task<IEnumerable<Client>> GetAll() => await _context.Clients.ToListAsync();

    public async Task<Client?> GetById(int id) => await _context.Clients.FindAsync(id);

    public async Task<Client> Create(Client newClient) {

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return newClient;
    }

    public async Task Update(int id, Client client) {

        var match = await GetById(id);

        if (match is not null) {

            match.Name = client.Name;
            match.PhoneNumber = client.PhoneNumber;
            match.Email = client.Email;

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