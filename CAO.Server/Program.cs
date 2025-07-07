using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using CAO.Server.Models;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAll", builder =>
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        )
    );
}
else
{
    var certPemPath = Path.Combine(AppContext.BaseDirectory, "certs", "certificate");
    var keyPemPath = Path.Combine(AppContext.BaseDirectory, "certs", "key");

    var cert = X509Certificate2.CreateFromPemFile(certPemPath, keyPemPath);
    builder.WebHost.UseKestrel(options =>
        options.ListenAnyIP(443, listenOptions =>
            listenOptions.UseHttps(cert)
        )
    );
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowCaoOrigins", builder =>
            builder.WithOrigins("https://codeart.online").AllowAnyMethod().AllowAnyHeader()
        )
    );
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CaoDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("AllowCaoOrigins");
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
