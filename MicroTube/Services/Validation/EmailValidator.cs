using System.Text.RegularExpressions;

namespace MicroTube.Services.Validation
{
    public class EmailValidator : IEmailValidator
    {
        //https://github.com/angular/angular/blob/main/packages/forms/src/validators.ts#L125
        const string REGEX = @"^(?=.{1,254}$)(?=.{1,64}@)[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        public IServiceResult Validate(string email)
        {
			Console.WriteLine("Validating email: " + email);
            if (email is null) 
                return ServiceResult.Fail(400, "Invalid email.");
            if (Regex.IsMatch(email, REGEX, RegexOptions.None))
                return ServiceResult.Success();
            return ServiceResult.Fail(400, "Invalid email.");
        }
    }
}
