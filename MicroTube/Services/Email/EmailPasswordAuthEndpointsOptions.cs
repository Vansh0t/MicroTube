namespace MicroTube.Services.Email
{
    public class EmailPasswordAuthEndpointsOptions
    {
        public const string KEY = "EmailPasswordAuthEndpoints";
        public required string EmailConfirmation { get; set; }
        public required string EmailChange { get; set; }
        public required string PasswordReset { get; set; }
    }
}
