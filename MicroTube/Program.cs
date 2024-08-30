using EntityFramework.Exceptions.SqlServer;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MicroTube;
using MicroTube.Constants;
using MicroTube.Data.Access;
using MicroTube.Data.Models;
using MicroTube.Services.Authentication;
using MicroTube.Services.Cryptography;
using MicroTube.Services.Email;
using MicroTube.Services.MediaContentStorage;
using MicroTube.Services.Search;
using MicroTube.Services.Validation;
using MicroTube.Services.VideoContent;
using MicroTube.Services.VideoContent.Processing;
using MicroTube.Services.VideoContent.Views;
using NSwag.Generation.Processors.Security;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Abstractions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
// Add services to the container.
builder.Services.AddAzureBlobStorage(config.GetRequiredValue("AzureBlobStorage:ConnectionString"));
builder.Services.AddSingleton<IMD5HashProvider, MD5HashProvider>();
builder.Services.AddSingleton<IVideoAnalyzer, FFMpegVideoAnalyzer>();
//builder.Services.AddSingleton<IVideoContentRemoteStorage<AzureBlobAccessOptions, BlobUploadOptions>, AzureBlobVideoContentRemoteStorage>();
builder.Services.AddSingleton<IVideoContentRemoteStorage<OfflineRemoteStorageOptions, OfflineRemoteStorageOptions>, OfflineVideoContentRemoteStorage>();
//builder.Services.AddSingleton<ICdnMediaContentAccess, AzureCdnMediaContentAccess>();
builder.Services.AddElasticsearchClient(config);
builder.Services.AddElasticsearchSearch();
builder.Services.AddSingleton<ICdnMediaContentAccess, OfflineCdnMediaContentAccess>();
builder.Services.AddSingleton<IEmailValidator, EmailValidator>();
builder.Services.AddSingleton<IUsernameValidator, UsernameValidator>();
builder.Services.AddSingleton<IPasswordValidator, DefaultPasswordValidator>();
builder.Services.AddSingleton<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddSingleton<IEmailManager, DefaultEmailManager>();
builder.Services.AddSingleton<IEmailTemplatesProvider, DefaultEmailTemplatesProvider>();

builder.Services.AddSingleton<IUserSessionService, DefaultUserSessionService>();
builder.Services.AddSingleton<IVideoPreUploadValidator, DefaultVideoPreUploadValidator>();
builder.Services.AddSingleton<IVideoNameGenerator, GuidVideoNameGenerator>();
//builder.Services.AddScoped<IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress>, AzureBlobVideoPreprocessingPipeline>();
builder.Services.AddScoped<IVideoPreprocessingPipeline<VideoPreprocessingOptions, VideoUploadProgress>, OfflineVideoPreprocessingPipeline>();
builder.Services.AddVideoReactions();
//builder.Services.AddScoped<IVideoProcessingPipeline, AzureBlobVideoProcessingPipeline>();
builder.Services.AddOfflineVideoProcessing();
builder.Services.AddDbContext<MicroTubeDbContext>(
	options => options.UseSqlServer(config.GetDefaultConnectionString())
					  .UseExceptionProcessor());
builder.Services.AddDefaultBasicAuthenticationFlow();
builder.Services.AddScoped<IVideoThumbnailsService, FFMpegVideoThumbnailsService>();
builder.Services.AddScoped<IVideoCompressionService, FFMpegVideoCompressionService>();
builder.Services.AddScoped<IAuthenticationEmailManager, DefaultAuthenticationEmailManager>();
builder.Services.AddScoped<IPasswordEncryption, PBKDF2PasswordEncryption>();
builder.Services.AddScoped<IVideoIndexingService, DefaultVideoIndexingService>();
builder.Services.AddScoped<IVideoViewsAggregatorService, DefaultVideoViewsAggregatorService>();
builder.Services.AddScoped<IFileSystem, FileSystem>();

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
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.GetRequiredValue("JWT:Key"))),
            ValidIssuer = config.GetRequiredValue("JWT:Issuer"),
            ValidAudience = config.GetRequiredValue("JWT:Audience")
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationConstants.PASSWORD_RESET_ONLY_POLICY, policy => policy.RequireClaim(AuthorizationConstants.PASSWORD_RESET_CLAIM));
    options.DefaultPolicy = new AuthorizationPolicyBuilder(options.DefaultPolicy).RequireClaim(AuthorizationConstants.USER_CLAIM).Build();
});
builder.Services.AddControllersWithViews();
builder.Services.AddOpenApiDocument(options =>
{
	options.AddSecurity("Bearer", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
	{
		Type = NSwag.OpenApiSecuritySchemeType.Http,
		Name = "Authorization",
		Description = "Default Auth",
		In = NSwag.OpenApiSecurityApiKeyLocation.Header,
		BearerFormat = "JWT",
		Scheme = "Bearer"
	});
	options.OperationProcessors.Add(new OperationSecurityScopeProcessor("Bearer"));
});
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy.WithOrigins(config.GetRequiredValue("ClientApp:URL"));
		policy.AllowAnyHeader();
		policy.AllowAnyMethod();
		policy.AllowCredentials();
	});
});
builder.Services.AddHangfire(hangfireConfig =>
{
	hangfireConfig.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UseSqlServerStorage(config.GetDefaultConnectionString())
	.UseColouredConsoleLogProvider()
	.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });//TODO: temp for development
	
});
builder.Services.AddHangfireServer(options =>
{
	options.ServerName = "VideoProcessing_1";
	options.WorkerCount = 1;
	options.Queues = new[] { "video_processing" };
});
builder.Services.AddHangfireServer(options =>
{
	options.ServerName = "VideoMetaProcessing_1";
	options.WorkerCount = 1;
	options.Queues = new[] { "video_indexing", "video_views_aggregation" };
});
//GlobalFFOptions.Configure(options => options.BinaryFolder = config.GetRequiredValue("FFmpegLocation"));
var app = builder.Build();



app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapHangfireDashboard(new DashboardOptions() { Authorization = new[] { new HangfireDashboardAnonymousAuthorizationFilter()} });
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
RecurringJob.AddOrUpdate<IVideoIndexingService>("VideoSearchIndexing", "video_indexing", service => service.EnsureVideoIndices(), Cron.Minutely);
RecurringJob.AddOrUpdate<IVideoViewsAggregatorService>("VideoViewsAggregation", "video_views_aggregation", service => service.Aggregate(), Cron.Minutely);
app.Run();
public partial class Program { }