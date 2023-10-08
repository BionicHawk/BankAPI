using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankAPI.Data.BankModels;
using BankAPI.Data.DTOs;
using BankAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BankAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase {

    private readonly LoginService _adminService;
    private IConfiguration _config;
    public LoginController(LoginService service, IConfiguration config) {
         _adminService = service;
         _config = config;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Login(AdminDTO adminDto) {
        
        var admin = await _adminService.GetAdmin(adminDto);

        if (admin is null) return Invalid();

        // Generar token
        string jwtToken = GenerateToken(admin);

        return Ok(new {token = jwtToken});

    }

    [HttpPost("client")]
    public async Task<IActionResult> ClientLogin(ClientDToIn clientDto) {
        
        var client = await _adminService.GetClient(clientDto);

        if (client is null) return Invalid();

        string jwtToken = GenerateClientToken(client);

        return Ok( new {token = jwtToken} );

    }


    private string GenerateToken(Administrator admin) {

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, admin.Name),
            new Claim(ClaimTypes.Email, admin.Email),
            new Claim("AdminType", admin.AdminType.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JWT:Key").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var securityToken = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds
        );

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;

    }

    private string GenerateClientToken(Client client) {

        var claims = new[] {
            new Claim(ClaimTypes.Name, client.Name),
            new Claim(ClaimTypes.Email, client.Email),
            new Claim("Client", "yes")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("JWT:Key").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var securityToken = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds
        );

        string token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;

    }

    private IActionResult Invalid() => BadRequest( new { message = "Credenciales inválidas" } );

}