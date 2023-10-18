using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MicroTube.Data.Access;
using MicroTube.Data.Access.SQLServer;
using MicroTube.Services.Authentication;
using MicroTube.Services.Authentication.Providers;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Users;
using MicroTube.Services.Validation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
// Add services to the container.
builder.Services.AddSingleton<IEmailValidator, EmailValidator>();
builder.Services.AddSingleton<IUsernameValidator, UsernameValidator>();
builder.Services.AddSingleton<IPasswordValidator, DefaultPasswordValidator>();
builder.Services.AddSingleton<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddSingleton<IAppUserDataAccess, AppUserDataAccess>();
builder.Services.AddScoped<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddScoped<IEmailPasswordAuthenticationDataAccess, EmailPasswordAuthenticationDataAccess>();
builder.Services.AddScoped<EmailPasswordAuthenticationProvider>();
builder.Services.AddTransient<IJwtTokenProvider, DefaultJwtTokenProvider>();
builder.Services.AddTransient<IJwtClaims, JwtClaims>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["JWT:Key"])),
            ValidIssuer = config["JWT:Issuer"],
            ValidAudience = config["JWT:Audience"]
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApiDocument();

var app = builder.Build();



app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
app.UseStaticFiles();
app.UseRouting();
app.MapFallbackToFile("index.html");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    
}
else
{
    app.UseOpenApi();
    app.UseSwaggerUi3();
}
app.Run();

public partial class Program { }