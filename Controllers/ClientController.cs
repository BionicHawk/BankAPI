using Microsoft.AspNetCore.Mvc;
using BankAPI.Services;
using BankAPI.Data.BankModels;
using Microsoft.AspNetCore.Authorization;

namespace BankAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase {

    private readonly ClientService _service;
    public ClientController(ClientService service) => _service = service;

    [Authorize(Policy = "ViewerAdmin")]
    [Authorize(Policy = "SuperAdmin")]
    [HttpGet("all")]
    public async Task<IEnumerable<Client>> GetClients() => await _service.GetAll();

    [Authorize(Policy = "ViewerAdmin")]
    [Authorize(Policy = "SuperAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<Client>> GetById(int id) {
        
        var client = await _service.GetById(id);

        if (client is null) { 

            return ClientNotFound(id);

        }

        return client;

    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpPost("create")]
    public async Task<IActionResult> Create(Client client) {

        var newClient = await _service.Create(client);

        return CreatedAtAction(nameof(GetById), new { id = newClient.Id }, newClient);
    }
    
    [Authorize(Policy = "SuperAdmin")]
    [HttpPut("modify/{id}")]
    public async Task<IActionResult> UpdateClient(int id, Client client) {
        
        if (!id.Equals(client.Id)) 
            return BadRequest(new { message = $"El ID {id} no coincide con el ID {client.Id} del cuerpo de la solicitud." });

        var clientToUpdate = await _service.GetById(id);

        if (clientToUpdate is not null) {

            await _service.Update(id, client);
            return NoContent();

        } else {

            return ClientNotFound(id);

        }

    }

    [Authorize(Policy = "SuperAdmin")]
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteClient(int id) {

        var clientToDelete = await _service.GetById(id);

        if (clientToDelete is not null) {

            await _service.Delete(id);
            return Ok();

        } else {

            return ClientNotFound(id);

        }

    }

    public NotFoundObjectResult ClientNotFound(int id) => NotFound( new { message = $"El cliente con ID {id} no existe." } );

}