using Kuari.AuthServer.Core.Configuration;
using Kuari.AuthServer.Core.Models;
using Kuari.AuthServer.Core.Repositories;
using Kuari.AuthServer.Core.Services;
using Kuari.AuthServer.Core.UnitOfWork;
using Kuari.AuthServer.Repository;
using Kuari.AuthServer.Repository.Repositories;
using Kuari.AuthServer.Repository.UnitOfWork;
using Kuari.AuthServer.Service.Services;
using Kuari.AuthServer.SharedLibrary.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//dl Register
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IService<,>),typeof(Service<,>));


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("Kuari.AuthServer.Repository");
    });
});

builder.Services.AddIdentity<UserApp, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.Password.RequireNonAlphanumeric = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders(); // identity db'nin dl'i

builder.Services.AddControllers();
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
builder.Services.Configure<List<Client>>(builder.Configuration.GetSection("Clients"));

// Token do�rulama mekanizmas�
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;// auth �ema ile client'tan gelecek jwtoken �eman�n birbiri ile e�le�tirilmesini yapt�k

}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    // ilk olarak appsettings.josn i�erisinde bulunan TokenOptions objesini CustomTokenOption class�na mapleyelim validasyonlarda laz�m olacak
    var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
    // Burada ise token�n �mr� ve di�er validasyonlar�n�  kontrol etmek i�in �nce validasyon parametreleri class�ndan instance olu�turmam�z gerekir.
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidIssuer = tokenOptions.Issuer,
        ValidAudience = tokenOptions.Audience[0],
        IssuerSigningKey = SignService.GetSymetricSecurityKey(tokenOptions.SecurityKey),

        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero

    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
