using System.Text;
using WebApplicationMail;

Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<MailingBackgroundService>();
var app = builder.Build();

async Task<string> SendMail(IEmailSender sender, string message, string subject)
{
    await sender.SendAsync("PV011", "income@rodion-m.ru", subject, message);
    return "OK";
}

app.MapGet("/sendmail", SendMail);

app.Run();