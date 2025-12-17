using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using CAO.Server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

var key = Encoding.ASCII.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CaoDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("CaoDatabase"))
);

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
