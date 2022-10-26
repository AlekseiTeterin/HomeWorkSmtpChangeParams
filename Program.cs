using Serilog;
using Serilog.Events;
using WebApplicationMail;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
    builder.Services.AddHostedService<MailingBackgroundService>();
    builder.Host.UseSerilog((ctx, conf) =>
    {
        conf
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.File("log-.txt", rollingInterval: RollingInterval.Hour)
            .ReadFrom.Configuration(ctx.Configuration)
            ;
    });
    var app = builder.Build();
    app.UseSerilogRequestLogging();
    async Task<string> SendMail(IEmailSender sender, string message, string subject)
    {
        await sender.SendAsync("PV011", "alexey_teterin89@mail.ru", subject, message);
        return "OK";
    }

    app.MapGet("/sendmail", SendMail);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush(); 
}