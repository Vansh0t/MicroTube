using MicroTube.Data.Access;
using MicroTube.Data.Access.SQLServer;
using MicroTube.Services.Authentication.Providers;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Users;
using MicroTube.Services.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IEmailValidator, EmailValidator>();
builder.Services.AddSingleton<IUsernameValidator, UsernameValidator>();
builder.Services.AddSingleton<IPasswordValidator, DefaultPasswordValidator>();
builder.Services.AddSingleton<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddSingleton<IAppUserDataAccess, AppUserDataAccess>();
builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddScoped<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddScoped<IEmailPasswordAuthenticationDataAccess, EmailPasswordAuthenticationDataAccess>();
builder.Services.AddScoped<EmailPasswordAuthenticationProvider>();


builder.Services.AddControllersWithViews();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.UseHttpsRedirection();
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
