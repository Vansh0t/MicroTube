using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MicroTube.Constants;
using MicroTube.Data.Access;
using MicroTube.Data.Access.SQLServer;
using MicroTube.Services.Authentication;
using MicroTube.Services.Authentication.Providers;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.Users;
using MicroTube.Services.Validation;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
// Add services to the container.
builder.Services.AddSingleton<IEmailValidator, EmailValidator>();
builder.Services.AddSingleton<IUsernameValidator, UsernameValidator>();
builder.Services.AddSingleton<IPasswordValidator, DefaultPasswordValidator>();
builder.Services.AddSingleton<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddSingleton<IAppUserDataAccess, AppUserDataAccess>();
builder.Services.AddSingleton<IEmailManager, DefaultEmailManager>();
builder.Services.AddScoped<IAuthenticationEmailManager, DefaultAuthenticationEmailManager>();
builder.Services.AddScoped<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddScoped<IEmailPasswordAuthenticationDataAccess, EmailPasswordAuthenticationDataAccess>();
builder.Services.AddScoped<EmailPasswordAuthenticationProvider>();
builder.Services.AddTransient<IJwtTokenProvider, DefaultJwtTokenProvider>();
builder.Services.AddTransient<IJwtPasswordResetTokenProvider, DefaultJwtPasswordResetTokenProvider>();
builder.Services.AddTransient<IJwtClaims, JwtClaims>();
builder.Services.AddTransient<ISecureTokensProvider, SHA256SecureTokensProvider>();
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationConstants.PASSWORD_RESET_ONLY_POLICY, policy => policy.RequireClaim(AuthorizationConstants.PASSWORD_RESET_CLAIM));
    options.DefaultPolicy = new AuthorizationPolicyBuilder(options.DefaultPolicy).RequireClaim(AuthorizationConstants.USER_CLAIM).Build();
});
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApiDocument();

var app = builder.Build();



app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
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