using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using BankAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BankTransactionController : ControllerBase {

    private readonly BankTransactionService _service;

    public BankTransactionController(BankTransactionService service) => _service = service;

    [HttpGet("GetMyAccounts")]
    public async Task<ActionResult<IEnumerable<Account>>> GetMyAccounts() {

        string? token = GetToken();

        if (token.IsNullOrEmpty()) return NoValidToken();

        var client = await GetClient(token!);
        var accounts = await _service.GetAccounts(client!);

        if (client is null) return NoValidToken();
        if (accounts is null) return Ok( new { message = "Este cliente no tiene cuentas" } );

        return accounts.ToList()!;

    }

    [HttpPut("RetireTransfer")]
    public async Task<ActionResult> RetireTransfer(RetireTransferDToIn retireTransferDToIn) {

        string? token = GetToken();

        if (token.IsNullOrEmpty()) return NoValidToken();

        var client = await GetClient(token!);
        var account = await _service.GetAccountByID(retireTransferDToIn.Id);

        if (client is null) NoValidToken();
        if (account is null) NoAccountFound();

        if (ClientOwnsAccount(client!, account!)) {

            if (!await _service.TryToRetireWithTransfer(account!, retireTransferDToIn.Amount, retireTransferDToIn.ExternalAccount)) {

                return BadRequest( new { message = "La cuenta no tiene saldo suficiente para realizar el retiro" } );

            }

            return NoContent();

        }

        return NotOwner();

    }

    [HttpPut("RetireCash")]
    public async Task<ActionResult> RetireCash(RetireCashDToIn retireCashDToIn) {
        
        string? token = GetToken();

        if (token.IsNullOrEmpty()) return NoValidToken();

        var client = await GetClient(token!);
        var account = await _service.GetAccountByID(retireCashDToIn.Id);

        if (client is null) return NoValidToken();
        if (account is null) return NoAccountFound();

        if (ClientOwnsAccount(client, account)) {

            if (! await _service.TryToRetireWithCash(account, retireCashDToIn.Amount)) {

                return BadRequest( new { message = "La cuenta no tiene saldo suficiente para realizar el retiro" } );
            
            }

            return NoContent();

        }

        return NotOwner();

    }

    [HttpPut("Deposit")]
    public async Task<ActionResult> Deposit(DepositDtoIn depositDtoIn) {
        
        string? token = GetToken();

        if (token.IsNullOrEmpty()) return NoValidToken();

        var client = await GetClient(token!);
        var account = await _service.GetAccountByID(depositDtoIn.Id);

        if (client is null) return NoValidToken();
        if (account is null) return NoAccountFound();

        if (ClientOwnsAccount(client, account)) {

            if (! await _service.TryToDeposit(account, depositDtoIn.Amount)) {

                return NoAccountFound();

            }

            return NoContent();

        }

        return NotOwner();

    }

    [HttpDelete("DeleteMyAccount/{id}")]
    public async Task<ActionResult> DeleteAccount(int id) {

        string? token = GetToken();

        if (token.IsNullOrEmpty()) return NoValidToken();

        var client = await GetClient(token!);
        var account = await _service.GetAccountByID(id);

        if (client is null) return NoValidToken();
        if (account is null) return NoAccountFound();

        if (ClientOwnsAccount(client, account)) {

            // La cuenta pertenece al cliente
            if (!await _service.TryDeleteAccount(account)) {
                return BadRequest( new { message = "La cuenta aún tiene saldo, por lo que no puede ser eliminada" } );
            }

            return Ok();

        }

        return NotOwner();

    }

    private bool ClientOwnsAccount(Client client, Account account) {

        int clientId = client.Id;
        int accountClientId = (int)account.ClientId!;

        return clientId.Equals(accountClientId);

    }

    private async Task<Client?> GetClient(string token) {
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claims = jwtToken.Claims;
        var email = claims.Single(claim => claim.Type == ClaimTypes.Email);

        return await _service.GetClient(email.Value);

    }

    private string? GetToken() {
        string? token = HttpContext.Request.Headers["Authorization"];
        if (!token.IsNullOrEmpty()) {
            if (token!.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                token = token.Substring("Bearer ".Length).Trim();
            }
        }
        return token;
    }

    private ActionResult NoValidToken() => BadRequest( new { message = "El token no es válido" } );
    private ActionResult NoAccountFound() => BadRequest( new { message = "La cuenta proporcionada no existe" } );
    private ActionResult NotOwner() => BadRequest( new { message = "La cuenta no pertenece al cliente" } );

}