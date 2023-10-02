using BankAPI.Data.BankModels;
using BankAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers;

[ApiController]
[Route("[Controller]")]
public class AccountController : ControllerBase {
    
    private readonly AccountService _service;
    private readonly ClientService _clientSevice;
    public AccountController(AccountService service, ClientService clientService) {
        _service = service;
        _clientSevice = clientService;
    }

    [HttpGet]
    public async Task<IEnumerable<Account>> GetAll() => await _service.GetAll();

    [HttpGet("{id}")]
    public async Task<ActionResult<Account>> GetById(int id) {

        var match = await _service.GetById(id);

        if (match is null) {

            return AccountNotFound(id);

        }

        return match;

    }

    [HttpPost]
    public async Task<IActionResult> Create(Account account) {

        if (account.ClientId is not null) {

            int id = (int)account.ClientId;

            if (await _clientSevice.GetById(id) is null) {
                return BadRequest( new { message = $"El ID del cliente {id} no existe!" } );
            }

        }

        var newAccount = await _service.Create(account);

        return CreatedAtAction(nameof(GetById), new { id = newAccount.Id } , newAccount);

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update (int id, Account account) {

        if (!id.Equals(account.Id)) 
            return BadRequest( new { message = $"El ID {id} no coincide con el ID {account.Id} del cuerpo de la solicitud." } );

	if (account.ClientId is not null) {
	  
	  int cId = (int)account.ClientId;
	  var clientMatch = await _clientSevice.GetById(cId);
	  if (clientMatch is null) return BadRequest( new { message = $"El cliente con el id {cId} no existe" } );

	}

        var match = await _service.GetById(id);

        if (match is not null) {

            await _service.Update(id, account);
            return NoContent();

        }

        return AccountNotFound(id);

    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id) {

        var match = await _service.GetById(id);

        if (match is not null) {

            await _service.Delete(id);
            return Ok();

        }

        return AccountNotFound(id);

    }

    public NotFoundObjectResult AccountNotFound(int id) => NotFound( new { message = $"La cuenta con el ID {id} no existe." } );

}
