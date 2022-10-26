using Polly;
using Polly.Retry;

namespace WebApplicationMail;

public class MailingBackgroundService : BackgroundService
{
    private readonly ILogger<MailingBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public MailingBackgroundService(
        IHostApplicationLifetime applicationLifetime,
        ILogger<MailingBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        applicationLifetime.ApplicationStarted.Register(() =>
        {
            _logger.LogInformation("Приложение успешно запущено");
        });
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var mailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        while (!stoppingToken.IsCancellationRequested)
        {
            var to = "alexey_teterin89@mail.ru";
            var retryCount = 2;

            AsyncRetryPolicy? policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryAttempt, context)=>
                //.RetryAsync(retryCount, onRetry: (exception, retryAttempt) =>
               {
                   _logger.LogWarning(
                    exception, 
                    "При попытке отправить письмо произошла ошибка, Попытка #{retryAttempt}",
                    retryAttempt
                    );
               });

            PolicyResult? result = await policy.ExecuteAndCaptureAsync(
                                       token => mailSender.SendAsync("Родион", to, "Привет", "Все хорошо"), stoppingToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                _logger.LogError(result.FinalException, "Произошла ошибка при попытке отправки письма");
            }
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        }
    }
}