using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.WebApi.Data;
using PersonalFinanceManager.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<PersonalFinanceManager.WebApi.Services.IAccountService, PersonalFinanceManager.WebApi.Services.AccountService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(80);
    options.ListenAnyIP(443);
/* options.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.UseHttps("/app/certificate.pfx", "mypassword"); // U¿yj œcie¿ki i has³a
    }); */
});

builder.Services.AddHttpClient(); // potrzebne do API zewnêtrznych
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
