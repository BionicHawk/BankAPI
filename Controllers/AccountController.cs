using BankAPI.Data.BankModels;
using BankAPI.Services;
using Microsoft.AspNetCore.Mvc;
using BankAPI.Data.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[Controller]")]
public class AccountController : ControllerBase {
    
    private readonly AccountService _service;
    private readonly ClientService _clientSevice;
    public AccountController(AccountService service, ClientService clientService) {
        _service = service;
        _clientSevice = clientService;
    }

    [Authorize(Policy = "ViewerAdmin")]
    [Authorize(Policy = "SuperAdmin")]
    [HttpGet("all")]
    public async Task<IEnumerable<AccountDToOut>> GetAll() => await _service.GetAll();

    [Authorize(Policy = "ViewerAdmin")]
    [Authorize(Policy = "SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDToOut>> GetById(int id) {

        var match = await _service.GetDtoById(id);

        if (match is null) {

            return AccountNotFound(id);

        }

        return match;

    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(AccountDToIn account) {

        if (account.ClientId is not null) {

            if (!await ValidateClientID((int)account.ClientId)) {
                return BadRequest( new { message = $"El cliente con el id {(int)account.ClientId} no existe"} );
            }

        }

        var newAccount = await _service.Create(account);

        return CreatedAtAction(nameof(GetById), new { id = newAccount.Id } , newAccount);

    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpPut("modify/{id}")]
    public async Task<ActionResult> Update (int id, AccountDToIn account) {

        if (!id.Equals(account.Id)) 
            return BadRequest( new { message = $"El ID {id} no coincide con el ID {account.Id} del cuerpo de la solicitud." } );

        if (account.ClientId is not null) {

            if (!await ValidateClientID((int)account.ClientId)) {
                return BadRequest( new { message = $"El cliente con el id {(int)account.ClientId} no existe"} );
            }

        }

        var match = await _service.GetById(id);

        if (match is not null) {

            await _service.Update(id, account);
            return NoContent();

        }

        return AccountNotFound(id);

    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpDelete("delete/{id}")]
    public async Task<ActionResult> Delete(int id) {

        var match = await _service.GetById(id);

        if (match is not null) {

            await _service.Delete(id);
            return Ok();

        }

        return AccountNotFound(id);

    }

    public NotFoundObjectResult AccountNotFound(int id) => NotFound( new { message = $"La cuenta con el ID {id} no existe." } );

    public async Task<bool> ValidateClientID(int id) {

        var match = await _clientSevice.GetById(id);
        return match is not null;

    }

}
