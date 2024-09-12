namespace MicroTube.Services.Email
{
    //"Email": {
    //    "SMTP": {
    //        "Address": "",
    //        "Email": "",
    //        "Password": "",
    //        "TLSPort": 587,
    //        "SSLPort": 465,
    //    },
    //    "TemplatesLocation": "/Templates/Email"
    //}
    public class EmailOptions
    {
        public const string KEY = "Email";
        public required SMTPOptions SMTP { get; set; }
        public required string TemplatesLocation { get; set; }
        public required string SenderAddress { get; set; }
        public required string Sender { get; set; }
    }
    public class SMTPOptions
    {
        public required string Server { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int TLSPort { get; set; }
        public int SSLPort { get; set; }
        public required string SenderDomain { get; set; }
    }
}
