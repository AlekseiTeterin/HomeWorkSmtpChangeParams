namespace WebApplicationMail;
#pragma warning disable CS8618
public class SmtpConfig
{
        public string Host { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
}

