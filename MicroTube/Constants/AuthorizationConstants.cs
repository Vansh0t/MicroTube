using System.Reflection.Metadata;

namespace MicroTube.Constants
{
    public static class AuthorizationConstants
    {
        public const string PASSWORD_RESET_ONLY_POLICY = "PasswordResetOnly";
        public const string PASSWORD_RESET_CLAIM = "password_reset";
        public const string USER_CLAIM = "user";
		public const string REFRESH_TOKEN_COOKIE_KEY = "refreshToken";
    }
}
