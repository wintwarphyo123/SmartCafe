using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using Scalar.AspNetCore;
using SmartCafe.Data;
using SmartCafe.Interfaces;
using SmartCafe.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();//socket
var epplusLicense = builder.Configuration["EPPlus:ExcelPackage:License"];
if (!string.IsNullOrEmpty(epplusLicense))
{
    ExcelPackage.License.SetNonCommercialPersonal(epplusLicense);
}

// 1. Existing registration for ApplicationDbContext (for Identity)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ADD THIS: Registration for your new scaffolded SmartCafeDbContext
builder.Services.AddDbContext<SmartCafeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
});

builder.Services.AddScoped<IJwtService, JwtService>();
// Add services to the container.

//Authentication, JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]!
                    )
                )
        };
});
builder.Services.AddScoped<ExportService>();

builder.Services.AddScoped<IConvertion, Convertion>();//Service folder
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IJwtService,JwtService>();
builder.Services.AddHostedService<OrderCleanService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference("/docs/scalar");
}
app.UseHttpsRedirection();

app.UseStaticFiles(); // This allows the server to serve images!

app.UseCors("AllowAngular");//to connect angular

app.UseAuthentication();

app.UseAuthorization();
app.MapHub<SmartCafe.Hubs.NotificationHubs>("/notificationHub");

app.MapControllers();

app.Run();
