using System.Text;
using BankAPI.Data;
using BankAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
builder.Services.AddSqlServer<BankContext>(builder.Configuration.GetConnectionString("BankConnection"));

// Service Layer for client
builder.Services.AddScoped<ClientService>();
// Service Layer for account
builder.Services.AddScoped<AccountService>();
// Service Layer for login
builder.Services.AddScoped<LoginService>();
// Service Layer for BankTransaction
builder.Services.AddScoped<BankTransactionService>();
// Sevice of JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("AdminType", "1"));
    options.AddPolicy("ViewerAdmin", policy => policy.RequireClaim("AdminType", "2"));
    options.AddPolicy("ClientUser", policy => policy.RequireClaim("Client", "yes"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
